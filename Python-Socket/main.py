#Imports modules
import socket
import time

listensocket = socket.socket() #Creates an instance of socket
Port = 8000 #Port to host server on
maxConnections = 999
IP = socket.gethostname() #IP address of local machine

listensocket.bind(('',Port))

#Starts server
listensocket.listen(maxConnections)
print("Server started at " + IP + " on port " + str(Port))

#Accepts the incomming connection
(clientsocket, address) = listensocket.accept()
print("New connection made!")

running = True



while running:
    message = clientsocket.recv(1024).decode() #Gets the incomming message
    print(message)
    if not message == "":
        time.sleep(5)