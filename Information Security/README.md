# Information Security – Encrypted Chat Application

This project is a peer-to-peer Windows Forms chat application that allows two users to exchange messages over TCP/IP using .NET sockets, with encryption and decryption implemented through the Bifid cipher.

## Features

- Real-time TCP/IP communication between two users (server ↔ client)
- Sending and receiving text messages
- Message encryption using the Bifid cipher
- Custom key support for encryption (validated before use)
- Automatic removal of invalid characters from input
- Simple and intuitive WinForms GUI
- Key validation (maximum 25 letters, no duplicates)

## Technologies

- C# (.NET Framework / WinForms)
- System.Net.Sockets for network communication
- Custom Bifid Cipher implementation

## How to Run

1. Clone the repository or download the project files.
2. Open the project in **Visual Studio**.
3. Run the application (F5).
4. In one window, enter the servers port number and click **Start** to launch the server.
5. In another window, enter the server’s port, then click **Connect**.
6. Type a message and (optionally) a key, then click **Send**.
7. Messages will appear in the list box. If a key is set, messages will be encrypted. Both windows need to have same key, otherwise encryption won't work

