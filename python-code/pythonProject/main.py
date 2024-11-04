import socket
import asyncio
from bleak import BleakScanner
from datetime import datetime

# Global variable to store the current client sockets
client_sockets = {}

async def start_server():
    global client_sockets
    client_counter = 1
    server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    server_socket.bind(('LAPTOP-E2THQTEG', 8000))
    server_socket.listen(5)
    print(f'Server is listening on LAPTOP-E2THQTEG:8000')

    while True:
        client_socket, address = await asyncio.get_event_loop().run_in_executor(None, server_socket.accept)
        client_id = str(client_counter)
        client_counter += 1  # Increment the counter for the next client
        print(f'Connection from {client_id}')

        # Store the client socket with the client_id
        client_sockets[client_id] = client_socket


        # Start a new task to handle the client
        asyncio.create_task(handle_client(client_socket, client_id))

async def handle_client(client_socket, client_id):
    while True:
        try:
            data = await asyncio.get_event_loop().run_in_executor(None, client_socket.recv, 1024)
            if not data:  # Exit the loop if no data is received (client disconnected)
                print("Client disconnected.")
                break


            # Send acknowledgment back to the client
            response = 'Message received!'
            await asyncio.get_event_loop().run_in_executor(None, client_socket.send, response.encode())

            await asyncio.sleep(5)  # Optional delay for a follow-up message

            # Send a follow-up message after 5 seconds
            second_response = 'This is your follow-up message after 5 seconds!'
            await asyncio.get_event_loop().run_in_executor(None, client_socket.send, second_response.encode())

            print(f'Received: {data.decode()}')
            response_model = data.decode()
            await asyncio.get_event_loop().run_in_executor(None, client_socket.send, response_model.encode())

            message=data.decode()

            if message.startswith("SENDTO:"):
                # Example format: "SENDTO:targetid|Hello, Target!"
                target_info, msg_content = message.split('|', 1)
                # Extract "targetid" from "SENDTO:targetid"
                _, target_id = target_info.split(':', 1)
                if target_id in client_sockets:
                    target_socket = client_sockets[target_id]
                    await asyncio.get_event_loop().run_in_executor(None, target_socket.send, msg_content.encode())
                    print(f"Message sent to {target_id}: {msg_content}")
                else:
                    print(f"Client {target_id} not connected.")
                    client_socket.send("Target client not connected.".encode())
        except Exception as e:
            print(f"Error handling client: {e}")
            break

    # Close the client socket after the loop ends
    client_socket.close()

async def scan_bluetooth_devices():
    global client_sockets
    while True:
        print("Starting Bluetooth scan...")
        try:
            devices = await BleakScanner.discover()
            scan_time = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
            print(f"\nScan completed at {scan_time}")
            print(f"Found {len(devices)} devices:\n")

            specific_mac = "A4:C6:9A:A3:EA:F2", "A4:C6:9A:A3:EA:F2"
            for device in devices:
                print(f"Device Name: {device.name or 'Unknown'}")
                print(f"MAC Address: {device.address}")

                if device.address in specific_mac:
                    message = f"{device.address}"
                    print(f"Sending message to client: {message}")

                    for client_id, client_socket in client_sockets.items():
                        if client_socket:  # Ensure there's an active client connection
                            await asyncio.get_event_loop().run_in_executor(None, client_socket.send, message.encode())
                        else:
                            print(f"No active client connection to send the message to client {client_id}.")

            await asyncio.sleep(5)  # Wait before the next scan

        except asyncio.CancelledError:
            print("Bluetooth scan task was cancelled.")
            break
        except Exception as e:
            print(f"An error occurred during Bluetooth scan: {str(e)}")
            await asyncio.sleep(5)

async def main():
    server_task = asyncio.create_task(start_server())
    scan_task = asyncio.create_task(scan_bluetooth_devices())

    await asyncio.gather(server_task, scan_task)

# Entry point
if __name__ == "__main__":
    asyncio.run(main())
