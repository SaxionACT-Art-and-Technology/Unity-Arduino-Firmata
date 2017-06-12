Arduino Firmata implementation for Unity: 'Object-Oriented Arduino'
===================================================================

Arduino   
-------   
   
* Upload Standard Firmata (included with Arduino) to your board and use Unity with sensors and actuators. 

* Find the name (OSX) or number (Windows) of the Serialport under Tools/Port. Remember the numeric part at the end. You need to add this in the Unity script.


## Unity (tested in Unity 5.6.1)##

* Make a new project or use the project you are currently working in. 
* Go to Assets > Import Package ...
* Select the OOArduino.unitypackage
* Import.

![screen shot 2017-06-12 at 16 50 03](https://user-images.githubusercontent.com/1760616/27042172-7d3ba672-4f96-11e7-976a-1d3a72d4de85.png)

After import you'll probably see a lot of red warning signs in the console.   

![screen shot 2017-06-12 at 16 50 33](https://user-images.githubusercontent.com/1760616/27042163-776688ca-4f96-11e7-8d67-a563182c2f9f.png)

Like: `error CS0234: The type or namespace name 'Ports' does not exist in the namespace 'System.IO'`    
Thats because you have to configure the 'Api Compatibility Level' to '.NET 2.0'. To do that:

* Go to File > Build Settings ...
* Then Player Settings (it opens in the Inspector at the right side). 
* Click 'Other Settings'
* You can find 'Api Compatibility Level' under **Configuration**
* Select '.NET 2.0' (by default it's '.NET 2.0 Subset')

And the errors will disappear. 

* Then Select the Main Camera (actually you can attach a script to something else as well) in the Hierarchy. 
* Add Component (inspector)
* Scripts > OOArduino > Arduino Controller
* Press play
* Now you probaly see a log that says 'Started'

## ArduinoController: Doing something with Inputs and Outputs


* ArduinoController.cs is the script you need to modify. 
* Modify the constants (like `LED1_PIN == 05`) to your own needs.
* Change the pinConfiguration array to make it fit your own Arduino configuration. 
* Do all the writing / reading of the pins within the `if( arduino.IsConnected() )` statement. 

---

Check [Firmata.org](http://www.firmata.org/) for more info about Firmata. 
