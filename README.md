# Mastermind
A web based application implementing the Mastermind number guessing game as a REST API. All game state is maintained on the server. A simple Vue User Interface is provided for game play. A Swagger UI is provided to verify functionality.

#### _A web-based application to play Mastermind, a code-breaking game._

#### By **Jessica R. Williams**

## Table of Contents

1. [Technologies Used](#technologies)
2. [Description](#description)
	2a. [Game Rules Implements](#rules)
3. [Setup/Installation Requirements](#setup)
4. [Game Play Instructions](#gameplay)
5. [Discussion of the Code](#code)
6. [Known Bugs](#bugs)
7. [License](#license)
8. [Contact Information](#contact)

## Technologies Used <a id="technologies"></a>

* _C#_
* _.NET 8_
* _Swashbuckle for Swagger UI_
* _Vue 3_
* _Vite_
* _Random dot org HTTP Integer Generator API_

## Description <a id="description"></a>

This project exposes a backend and a frontend for a Mastermind game. The server generates a secret combination by calling the Random dot org Integer Generator API and keeps all game state in memory through a caching abstraction. Players interact through the simple Vue UI or through the included Swagger UI.

### Game Rules Implemented <a id="rules"></a>

* The secret consists of four digits in the inclusive range zero to seven
* The player has ten attempts to guess the secret combination
* After each guess the server returns counts of correct positions and correct numbers without revealing which digits are correct

## Setup/Installation Requirements <a id="setup"></a>

** For support with setup or installation issues [Contact Information](#contact)

### Install .NET8

* Open the terminal on your local machine
* If you aren't sure whether [.NET8](https://docs.microsoft.com/en-us/dotnet/) is installed on your local device, run `dotnet --version`
	* If you see any version of 8.x.x you're good to go on the .NET SDK front
	* If you see a message such as "command not found" or "dotnet is not recognized" you'll need to install it
* To install (or upgrade to) .NET8 for your operating system:
	* [Microsoft's Download Page](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
	* [Microsoft's Installation Instructions](https://learn.microsoft.com/en-us/dotnet/core/install/)
	* If you run into installation issues ChatGPT is great resource for troubleshooting or even asking for step by step instructions.
	* On a Mac, if you're already comfortable with Homebrew [Homebrew Formulae](https://formulae.brew.sh/formula/dotnet@8) else follow the Microsoft Installation Instrutions
* No database is required
* No API key is required for Random dot org when using the HTTP integers endpoint with plain text response

### Install Node.js and NPM

* If you're unsure whether you have [Node.js](https://nodejs.org/en) and [NPM] installed on our local device, or which versions run `node -v` and `npm -v`
	* You're probably safe with Node 20 or newer
* If you need to install Node.js: [Windows](https://nodejs.org/en/download) [Mac](https://formulae.brew.sh/formula/node)
	* Installing node also installs NPM

### Clone the project

* Navigate to the directory inside of which you wish to house this project
* Clone this project with the command `git clone https://github.com/jessicarubinwilliams/Mastermind.git`
* Navigate to `Mastermind/src/Mastermind.Api`
* Recreate project server environment and install required dependencies with terminal command `dotnet restore`
* Build the project's server with terminal command `dotnet build`

### Run the project

#### The Server
* In your terminal navigate to `Mastermind/src/Mastermind.Api`
* Recreate project server environment and install required dependencies with terminal command `dotnet restore`
* Build the project's server with terminal command `dotnet build`
* Run the server with the command `dotnet run`     

#### The Client
* In a second terminal navigate to `Mastermind/src/Mastermind.Client`
* Recreate project server environment and install required dependencies with terminal command `npm i`
* Build the project's server with terminal command `npm build`
* Run the Client with the command `npm start`

## Game Play Instructions<a id="gameplay"></a>
1. Click the button to start the game
2. Enter your first guess in the provided inputs
3. Click the button to submit your guess
4. Analyze the feedback you receive
5. Guess again, rinse and repeat


## Known Bugs <a id="bugs"></a>
* No known bugs

## Discussion of the Code <a id="code"></a>
See the repo's [Wiki](https://github.com/jessicarubinwilliams/Mastermind/wiki)

## License <a id="license"></a>
*[MIT](https://choosealicense.com/licenses/mit/)*

Copyright (c) **2025 Jessica R. Williams**

## Contact Information <a id="contact"></a>
**[Jessica R. Williams](mailto:jessicarubinwilliams@gmail.com)**