# Rethought.FrequentlyAskedQuestions

Tired of answering the same questions all the time in your Discord server? Now you can add a bot that will answer frequently asked questions!

**Please note that due to a lack of time I was neither able to completely finish the project nor provide a running demo at all times. If you want to demo the bot feel free to message me privately on Discord (Brian#1207).**

I will add a polished and hosted version of this eventually. Right now it is self hosting only.
A comprehensive guide on how to do that will follow shortly. 
If you decide to attempt this on your own (not recommended) you need to add an appsettings.json file and your google auth file as json in the project folder (not the solution folder). Other than that you need to change unfortunately hard coded intent ids of the CorrectionIntent, ShowAllIntent and AddIntent and add corresponding training phrases on Dialogflow.

## Features

- Understands the same question in different variations using AI
- Easily add new questions through natural language
- Correct questions and add new training phrases
- Dynamically train and learn as it is getting used through reactions

### Screenshots

The images are a bit big, so I put links instead of showing them.

https://i.imgur.com/nuHGAvn.png


https://i.imgur.com/77xeIxA.png




## Limitations

- Currently its not possible to untrain phrases after they have been trained unless you use the Dialogflow dashboard
- Currently its not possible to remove questions

Both of these will be fixed as soon as possible.

## Roadmap

(Descending priority)
- Fix above limitations
- Paid service to plug and play the bot
- Docker images for easier self hosting
- Quality of Life changes

