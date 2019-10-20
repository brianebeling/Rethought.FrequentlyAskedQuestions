# Rethought.FrequentlyAskedQuestions

Tired of answering the same questions all the time in your Discord server? Now you can add a bot that will answer frequently asked questions!

**This project was part of a hackathon. Due to a lack of time I was neither able to completely finish the project nor provide a running demo. This project has been discontinued, but can be compiled and run.**

## Setup

- Configure appsettings.json file and add your google auth file as json in the project folder **not the solution folder**.

- Change the (unfortunately) hard coded intent ids of the CorrectionIntent, ShowAllIntent and AddIntent and add corresponding training phrases on Dialogflow.

## Features

- Understands the same question in different variations using AI
- Easily add new questions through natural language
- Correct questions and add new training phrases
- Dynamically train and learn as it is getting used through reactions

### Screenshots

The images are a bit big, so I put links instead of showing them.

Screenshot of the bot answering questions

https://i.imgur.com/nuHGAvn.png


Screenshot of someone adding a new question and then testing with a variation.
Additionally that person has clickeed on the reaction and the bot has been trained on their variation.

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

