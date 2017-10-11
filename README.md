# Merlin - Albion Online BOT

## Merlin is currently working with Albion 1.0.332.98729

This project started as work of one guy, now there a whole open community developing it. Its free and you can download it any time from this github. Merlin its a project that automates (BOT) your actions inside the game Albion Online.

The game code, scripts and Merlin itself is writed in C#, and the Injector in C++. But you can also use your own Injector.

Keep in mind that this tool can be against the Albion Online ToS and you can be banned for using it.

### Features Supported:
 * Combat mobs
 * Automatic gathering
 * Automatic banking 

![screenshot from 2017-10-11 11-09-54](https://user-images.githubusercontent.com/1520059/31459703-587317c2-ae9a-11e7-8cb4-923aa9512375.png)

Have some problems? Modified something you want to share? Create issues or pull requests.

### TO-DO:

We have a [Trello board](https://trello.com/b/eGLVeGbL/merlin) where you can see our current todo.

### Discord - Albion Online development

Join us! :+1:

```javascript
https://discord.gg/Z4Qtjty
```

We have a [Discord Channel](https://discord.gg/Z4Qtjty) community to code tools for Albion Online.

### How it works

The code inside the Albion folder is a .NET solution. The main projects there are:
* Merlin.API: project that contains some code from a decompiled version of the real game code (Albion)
* Merlin: project that contains all the "bot logic" (it uses the Merlin.API project for that). As the game was built using the [Unity Engine](https://unity3d.com/pt) the bot code it's nothing than Unity code and C#. 

So when you compile this solution (described below) a `Merlin.dll` is generated (containing the bot logic). The next step is to inject this code inside the game process. This is made using the `injector.exe` (also provided in the repo). The `injector.exe` is a compiled program that uses of the [MInject project](https://github.com/EquiFox/MInject) (a mono injection library).

### Development Requirements (compile yourself):

 * Visual Studio 2017
 * .NET Framework 3.5
 * Windows 7 or above
 * Injector? There one provided in this github.
 * DLLs from Albion Online (we do not provide them)

For a more detailed "how to install" guide look [here](https://github.com/klukule/merlin/wiki/%5BMerlin%5D-How-to-Download-&-Install)
  
### Runtime Requirements (GUI)
 
 * Compiled binaries (previous step)
 * .NET Framework 3.5 (shiped with Windows 7 and above)
 
 
This project is licensed under [CC BY-NC 3.0](https://creativecommons.org/licenses/by-nc/3.0/legalcode) license.

