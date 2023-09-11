# EBS

This is a billing system I developed using Asp.net mvc 5 with C# as backend language, SQL server and ADO.NET as a data access Layer.
The system Currently has the following Features:
1. Login
2. Customers
3. Billing/ Invoicing
4. Payment
5. Receipts
6. Report Generation in pdf format.

The login is hard coded and there's no page to create users or manage users. Used BCrypt.NET to hash the password.
The Customers Feature involves anything related to customers from registering them to updating their data and stuff like that.
The Billing Feature is the most important part of the application and that is where we deal with anything related to billing customers.
Payment is a feature that handles payments and balances and stuff like that. it provides the ability to download payment records in bulk and each receipt individually.


