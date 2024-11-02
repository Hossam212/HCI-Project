import socket
import time

async def run_socket_server():
    listensocket = socket.socket()
    port = 8000
    max_connections = 999
    ip = socket.gethostname()

    listensocket.bind(('', port))
    listensocket.listen(max_connections)
    print("Server started at " + ip + " on port " + str(port))

    clientsocket, address = listensocket.accept()
    print("New connection made!")

    while True:
        message = clientsocket.recv(1024).decode()
        if message:
            print(message)
            time.sleep(5)
