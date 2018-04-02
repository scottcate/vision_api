# Twitter Vision Bot: @vision_api

I wanted to experiement with Azure Congnitive Services, Vision API. SO I thought it would be fun to make a social account that could talk to anyone interested. So @Vision_API on twitter was born. If tagged on a tweet (or reply) with a photo, the service will reply with an description of the photo.

## Getting Started: Consumer

Simply ues twitter and tag the account @vision_api. ( #vision_api hashtag also works). Wait 30 seconds, and the bot should have replied to you with a description of the photo. If Media (a photo) isn't found on the actual tweet tagged, then the service will look up the thread (all the replies of the replies) and will return a description of the first photo found. This is fun to then reply to any photo on twitter to see the machines description.

The rest of this README addresses developers interested in also playing with the Azure Cog Services.

## Getting Started: Developer

If you would like to experiment with the Azure Vision API, you'll need a few items to get started, all of which can be attained for the low cost of $0.00

### Prerequisites

* Azure Account: [Free Trials Available](https://cda.ms/nR "Free or Paid")

### Installing

Once the code is cloned, you'll find a [local.settings.json](./src/local.settigns.json) file in the root. This file has all the variables you'll need to populate to get started. You probably won't need to post results back to twitter, if you're just trying to use the Vision API. So the three keys you'll want to complete are ...


* AzureWebJobsStorage
* AzureVisionApiSubscriptionKey
* AzureVisionUriBase

These values will be retrieved in your code with the static method

```
GetEnvironmentVariable("<< key name >>")
```

### Analyizing a photo

This example is posting a photo that is already uploaded from twitter. So the method isn't uploading a byte[] (that overload is available, just not used in this demo). Post that to the helper method and you'll get back a [VisionDescription](https://github.com/scottcate/vision_api/blob/master/src/Models/VisionDescription.cs).

```
FetchVisionDescriptionAsync(TwitterStatus tweet, TwitterMedia media)
```

All the method really does is post a small JSON body of the url of the photo.

```
{"url":"<< fully qualified url here >>"}
```

To the 

## Built With

* [Azure Vision API](https://cda.ms/n2)
* [Azure Functions](https://cda.ms/nM)
* [Azure Function :: Timer](https://cda.ms/nN)
* [Azure Function :: Queue Trigger](https://cda.ms/nP)
* [Azure Table Storage](http://https//cda.ms/nQ)
* [TweetMoa Sharp (Forked from Tweet Sharp)](https://github.com/Yortw/tweetmoasharp)


## Authors

* **Scott Cate** - *Initial work* - [Twitter/@ScottCate](https://twitter.com/scottcate)

See also the list of [contributors](https://github.com/scottcate/vision_api/graphs/contributors) who have (mauybe someday) participated in this project.

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details

## Acknowledgments

* Special thanks to the [contributors of Tweet Moa Sharp](https://github.com/Yortw/tweetmoasharp/graphs/contributors) - this would have been much harder without their work
* Twitter friends who endured my countless #vision_api test tweets :)
* You (for trying this and submitting feedback) :)

# Enjoy!

