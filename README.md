# tweets-steaming analysis

This application is .net core 3.1 console application which provides the endpoints to steam the twitter feed and read the data contineously.

You can post your tweet using this application.

Below are step you need to perform before running the application .

Step 1 : Go to twitter developer console and create a a project and https://developer.twitter.com/

Step 2 : Now take below keys and secret from the settings:

         * ConsumerKey
         * ConsumerSecret
         * AccessToken
         * AccessSecret
once all done then update the web.config file and run the application it will give the real time tweets. It has max limit of 200 tweets per minute.

For free account / developer account you can't access more than 200 tweets / minute.
