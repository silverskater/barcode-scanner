# Easy Barcode Scanner

A C# .NET appliation that runs in the background and forwards barcode scanner input to a configured URL for shipping parcels.
The barcode scanner device needs to be in USB-COM-STD mode (serial port), driver: https://cdn.datalogic.com/Download?iddwnfile=22082.

Automates the generation and printing of [FAN Courier](https://www.fancourier.ro/) shipping labels (AWB) using the [SelfAWB](https://www.selfawb.ro/new/login) integration API.

See [SelfAWB API-Docs](https://github.com/FAN-Courier/API-Docs) for a public test account.
