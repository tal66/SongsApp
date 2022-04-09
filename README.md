# Songs-app

## Search + play songs ⋆ files or spotify ⋆ lyrics summary <br><br>

- find+play mp3 files
- find+open song's spotify link
- get lyrics
- get song summary (details below)

Edit configuration to use services (only the lyrics service doesn't require a token currently). <br><br>

![Alt demo](pics/SongsDemo.gif)

<br>

*update: turns out spotify's search also works with lyrics keywords.

<br>

### lyrics summary

I use a beta version of azure's text analytics to get a summary of the song.
Pretty sure it wasn't meant for this :). Anyway, the results depend heavily on punctuation marks.<br><br>

<br>

![Alt demo](pics/SongsDemo2.gif)
