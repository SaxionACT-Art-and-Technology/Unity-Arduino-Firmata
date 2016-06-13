// ------------------------------------------------------------------------------
// ArduinoController.cs
//
// Released under MIT license: http://www.opensource.org/licenses/mit-license.php
// Copyright (C) 2015   Douwe A. van Twillert - Art & Technology, Saxion
// ------------------------------------------------------------------------------

using System;
using System.IO;
using System.IO.Ports;
using System.Collections;

using UnityEngine;


namespace OOArduino {
	public class ArduinoController : MonoBehaviour {
		const int   BUTTON_PIN =  3;
		const int     LED1_PIN = 05;
		const int     LED2_PIN = 06;
		const int     LED3_PIN = 12;
		const int     LED4_PIN = 13;
		const int     LED5_PIN = 11;
		const int     LED6_PIN = 10;
		const int POTMETER_PIN =  0;

		protected Arduino         arduino;
		private   int           frameCount = 0;
		private   int        potmeterValue = 0;
		private   Boolean    isLedOn = false;
		private   Boolean  isButtonPressed = false;
		private   Boolean wasButtonPressed = false;

		string[] pinConfiguration = {
			null, 		   // Pin 0   null (is RX)
			"", 		   // Pin 1   null (is TX)
			"digitalIn",   // Pin 2   digitalIn or digitalOut
			"digitalIn",   // Pin 3   pwmOut or digitalIn or digitalOut
			"digitalOut",  // Pin 4   digitalIn or digitalOut
			"pwmOut",      // Pin 5   pwmOut or digitalIn or digitalOut
			"pwmOut",      // Pin 6   pwmOut or digitalIn or digitalOut
			"servo",       // Pin 7   digitalIn or digitalOut
			"servo",       // Pin 8   digitalIn or digitalOut
			"digitalOut",  // Pin 9   pwmOut or digitalIn or digitalOut or servo
			"pwmOut",      // Pin 10  pwmOut or digitalIn or digitalOut or servo
			"pwmOut",      // Pin 11  pwmOut or digitalIn or digitalOut
			"digitalOut",  // Pin 12  digitalIn or digitalOut
			"digitalOut",  // Pin 13  digitalIn or digitalOut ( led connected )
			"analogIn",    // Pin 14  (==Analog 0)
			"analogIn",    // Pin 15  (==Analog 1)
			"analogIn",    // Pin 16  (==Analog 2)
			"analogIn",    // Pin 17  (==Analog 3)
			"analogIn",    // Pin 18  (==Analog 4)
			"analogIn"     // Pin 19  (==Analog 5)
		};

		// Use this for initialization
		void Start () {
			arduino = new Arduino( pinConfiguration , "/dev/cu.usbmodem1421" , 57600 );
			//arduino = new Arduino( "/dev/cu.usbserial-A900cf4M" , 57600 );
			Debug.Log( "Started" );
		}


		void Update () {
			try {

				arduino.WriteUpdate();
				frameCount++;
				arduino.ReadUpdate();
			}
			catch ( Exception exception ) {
				Debug.Log( "Exception caught" );
				Debug.Log( "Exception = " + exception );
				Debug.Log( "stack trace = " + exception.StackTrace );
			}
			if ( arduino.IsConnected() ) {
				potmeterValue = arduino.ReadAnalog( POTMETER_PIN );
				isButtonPressed = arduino.ReadDigital( BUTTON_PIN );

				if ( isButtonPressed && !wasButtonPressed ) {
					isLedOn = !isLedOn;
				}

				if ( frameCount % 100 ==  5 ) arduino.WriteDigital( LED4_PIN, true  );
				if ( frameCount % 100 == 55 ) arduino.WriteDigital( LED4_PIN, false );
				arduino.WritePWM( LED1_PIN,       potmeterValue / 4 );
				arduino.WritePWM( LED2_PIN, 255 - potmeterValue / 4 );
				arduino.WritePWM( LED5_PIN,       frameCount % 256   );
				arduino.WriteDigital( LED3_PIN, isButtonPressed  );
				arduino.WritePWM( LED6_PIN, isLedOn ? 192 : 24 );

				wasButtonPressed = isButtonPressed;
			}
		}
		
		void LateUpdate () {
			try {
				arduino.WriteUpdate();
			}
			catch ( Exception exception ) {
				Debug.Log( "Exception caught" );
				Debug.Log( "Exception = " + exception );
				Debug.Log( "stack trace = " + exception.StackTrace );
			}
		}
	}
}


