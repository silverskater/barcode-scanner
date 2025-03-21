# Easy Barcode Scanner

A C# .NET appliation that runs in the background and supports a barcode scanner device and automatically prints shipping labels.
The barcode scanner device needs to be in USB-COM-STD mode (serial port), driver: https://cdn.datalogic.com/Download?iddwnfile=22082.

When scanning a barcode the corresponding AWB Tracking Number (Air Waybill Tracking Number) is automatically retrieved from a configurable custom API endpoint. 
This AWB is than used to retrieve and print the [FAN Courier](https://www.fancourier.ro/) shipping label using the [SelfAWB](https://www.selfawb.ro/new/login) integration API.

Version 2.0.0+ supports FANCourier API v2.0 (JSON based responses) and Bearer Token for the authentication and authorization.

[FAN Courier API v2.0 Docs](https://www.fancourier.ro/wp-content/uploads/2023/07/EN_FANCourier_API-2.0-160523.pdf)

Note: In order to use the selfAWB integration API, you must have a signed contract with [FAN Courier](https://www.fancourier.ro/).
