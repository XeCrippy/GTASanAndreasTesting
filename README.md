# GTA San Andreas teleport testing
* This was a quick tool i made for testing. I just put this here for documentation in case someone can make use of it. I'm working on figuring out how to teleport into interiors and how to teleport while in a vehicle as well as cleaning up the god mode/one hit kill so i can add it to my main tool here https://github.com/XeCrippy/Retro360
# Make sure to inject teleport hook while paused or loading or it may crash!

* I've been unable to find a static pointer for player coordinates so I wrote a little ppc function to pull the value from the register on execution and write it to free memory
* God mode one hit kill hasn't been fully tested. I did fix a few issues with it so now you won't die inside buildings and you won't take damage from bmx/motorcycle crashes but you will do on vehicle explosion

  
<a href="https://gyazo.com/c7671bd0fe5f392e45fafc7f18fe3448"><img src="https://i.gyazo.com/c7671bd0fe5f392e45fafc7f18fe3448.png" alt="Image from Gyazo" width="355"/></a>
