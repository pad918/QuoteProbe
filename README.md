# QuoteProbe

(Ai generated for now (I am lazy))
QuoteSage is a simple quote search engine that utilizes a vector database to understand the meaning of search queries and find quotes in videos that have similar meanings. This project aims to provide users with a seamless and intuitive way to discover relevant quotes from a vast collection of videos.

## Features
    * Automatically add quotes from youtube videos and playlists.
    * Display quotes and embedded youtube video staring where the quote is said.
    

## Installation / building
To install and set up the QuoteProbe project locally, follow these steps:

    - Clone the repo
    - Open the project in visual studio 2022
    - Set system variables for the api keys and endpoints
    - Build and run the aplication from within vs 2022

## Api:s and system variables

| Environment Variable  |  Description   |
|-----------------------|----------------|
| `OPENAI_API_KEY` | Your key for the openai api |
| `QDRANT_API_KEY` | Your api key for the qdrant api, if the variable is unset it will not use any api key (eg if you have a local database)    |
| `QDRANT_ENDPOINT`| The uri of the qdrant database  |



## Known bugs and problems
    - The qdrant binding in semantic kernel has a lot of problems at the moment. Removing points and collections in the database does not work and throws an exception
    - Quotes have references to the w.rong video sometimes.
    - Bad error handlig causes a lot of problems when uploading new videos.
