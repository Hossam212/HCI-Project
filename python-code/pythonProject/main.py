import socket
import time
import asyncio
from bleak import BleakScanner
from datetime import datetime

# Global variable to store the current client socket
current_client_socket = None

async def start_server():
    global current_client_socket
    server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    server_socket.bind(('MohammedAdnan', 8000))
    server_socket.listen(5)
    print(f'Server is listening on MohammedAdnan:8000')

    while True:
        client_socket, address = await asyncio.get_event_loop().run_in_executor(None, server_socket.accept)
        print(f'Connection from {address}')

        # Store the current client socket
        current_client_socket = client_socket

        data = await asyncio.get_event_loop().run_in_executor(None, client_socket.recv, 1024)
        if not data:
            break
        print(f'Received: {data.decode()}')

        response = 'Message received!'
        await asyncio.get_event_loop().run_in_executor(None, client_socket.send, response.encode())

        await asyncio.sleep(5)  # Wait for 5 seconds
        second_response = 'This is your follow-up message after 5 seconds!'
        await asyncio.get_event_loop().run_in_executor(None, client_socket.send, second_response.encode())

async def scan_bluetooth_devices():
    global current_client_socket
    while True:
        print("Starting Bluetooth scan...")
        try:
            devices = await BleakScanner.discover()
            scan_time = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
            print(f"\nScan completed at {scan_time}")
            print(f"Found {len(devices)} devices:\n")

            specific_mac = "CA:31:7B:D6:91:A5"
            for device in devices:
                print(f"Device Name: {device.name or 'Unknown'}")
                print(f"MAC Address: {device.address}")

                if device.address == specific_mac:
                    message = f"Found device: {device.name} with MAC: {device.address}"
                    print(f"Sending message to client: {message}")

                    if current_client_socket:  # Ensure there's an active client connection
                        await asyncio.get_event_loop().run_in_executor(None, current_client_socket.send, message.encode())
                    else:
                        print("No active client connection to send the message.")

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