# Template overview

This Environment [Template](https://documentation.bunnyshell.com/docs/templates-what-are-templates) is a boilerplate for creating a new environment based on a web app stack using .NET 6 with SQL Server for the database to enable digital wallets in Web3 with Circle API.
**This web app is a full virtual wallet.**

The template provides the Bunnyshell configuration composed of 2 Components (database + front/backend) and the CRUD application that demonstrates how to manage wallets, send payments and more using Circle API.

## How to use this Template

To use the web app, you need: 
1. Import the template into your account.
2. Get a new API from [Circle](https://developers.circle.com/)
3. Add a new environment variable with the name _circleApi_ and add the API Key as a value.
4. Deploy your app.
5. Get the url and use.

Here is a video how the web app works: https://youtu.be/Vh0UOD41CcY 

## Functions

With the app (PayWave), you can:
- Create one or more wallets for user.
- Manage balance and blockchain address per wallet to receive money.
- Manage an address book to create recipients for transactions.
- Move money between accounts.
- Send money to third parties.
- Create payments links to share with friends.
- Integrate with apps and webs thanks to POST requests.

## Technologies

PayWave is built with:
- .NET 6 (it is a MVC web app).
- SQL Server
- [Circle API](https://developers.circle.com/)
- Javascript
