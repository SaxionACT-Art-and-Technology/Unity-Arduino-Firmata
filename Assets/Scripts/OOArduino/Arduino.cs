/* Arduino.cs
 *
 * Released under MIT license: http://www.opensource.org/licenses/mit-license.php
 * Copyright (C) 2013   Douwe A. van Twillert - Art & Technology, Saxion
 */


using System;
using System.IO;
using System.IO.Ports;
using System.Collections;

using UnityEngine;


namespace OOArduino {
	/**
	 * This is a convenience (proxy) class to easy programming for the Arduino.
	 * It uses the lowLevelArduino class and only shows necessary details
	 *
	 * @author Douwe A. van Twillert, Saxion
	 *
	 * TODO: CHECK if using analog pins as digital input (PORTC or PORT#=2) works.
	 */
	public class Arduino //extends EventDispatcher
	{
		private static uint[] _value2led_brightness = {
              0,   1,   1,   2,   2,   2,   2,   2,   2,   3,   3,   3,   3,   3,   3,   3,
              3,   3,   3,   3,   3,   3,   3,   4,   4,   4,   4,   4,   4,   4,   4,   4,
              4,   4,   4,   5,   5,   5,   5,   5,   5,   5,   5,   5,   5,   6,   6,   6,
              6,   6,   6,   6,   6,   7,   7,   7,   7,   7,   7,   7,   8,   8,   8,   8,
              8,   8,   9,   9,   9,   9,   9,   9,   10,  10,  10,  10,  10,  11,  11,  11,
              11,  11,  12,  12,  12,  12,  12,  13,  13,  13,  13,  14,  14,  14,  14,  15,
              15,  15,  16,  16,  16,  16,  17,  17,  17,  18,  18,  18,  19,  19,  19,  20,
              20,  20,  21,  21,  22,  22,  22,  23,  23,  24,  24,  25,  25,  25,  26,  26,
              27,  27,  28,  28,  29,  29,  30,  30,  31,  32,  32,  33,  33,  34,  35,  35,
              36,  36,  37,  38,  38,  39,  40,  40,  41,  42,  43,  43,  44,  45,  46,  47,
              48,  48,  49,  50,  51,  52,  53,  54,  55,  56,  57,  58,  59,  60,  61,  62,
              63,  64,  65,  66,  68,  69,  70,  71,  73,  74,  75,  76,  78,  79,  81,  82,
              83,  85,  86,  88,  90,  91,  93,  94,  96,  98,  99,  101, 103, 105, 107, 109,
              110, 112, 114, 116, 118, 121, 123, 125, 127, 129, 132, 134, 136, 139, 141, 144,
              146, 149, 151, 154, 157, 159, 162, 165, 168, 171, 174, 177, 180, 183, 186, 190,
              193, 196, 200, 203, 207, 211, 214, 218, 222, 226, 230, 234, 238, 242, 248, 255,
		};
		
		
		private LowLevelArduino _lowLevelArduino;
		
		/**
		 * Constructor. You must specify the pinNr configuration and can specify the tcp port
		 *
		 * @param pinConfiguration  Array with pinNr configuration, first two are null, others are { pwmOut or digitalIn or digitalOut or servo }
		 * @param tcpPort         	The tcp port to listen to, see also the as2arduinoGlue configuration file
		 * @param host              the host on which the serialproxy port can be found.
		 */
		public Arduino( string[] pinConfiguration , String portName, int baudRate )
		{
			_lowLevelArduino = new LowLevelArduino( pinConfiguration, portName , baudRate );
		}
		
		
		// ================
		// Public functions
		// ================
		
		// Getters and setters                                                                                                  /** Returns <code>true</code> if the connected Arduino is an Mega arduino. */
		public Boolean         IsMega()            { return _lowLevelArduino.IsMega();                            }  /** Returns the firmware version in a single number (2.3 translates to 23). Can be 0. */
		public uint            FirmwareVersion()   { return _lowLevelArduino.FirmwareVersion();                   }  /** Returns the name of the firmware, can be <code>null</code>. */
		public String          FirmwareName()      { return _lowLevelArduino.FirmwareName();                      }  /** Returns number of digital pins. */
		public uint            NrOfDigitalPins()   { return _lowLevelArduino.NrOfDigitalPins();                   }  /** Returns number of analog pins. */
		public uint            NrOfAnalogPins()    { return _lowLevelArduino.NrOfAnalogPins();                    }  /** Returns the total number of pins. Is the same as the sum of the number of analog and digital pins. */
		public uint            NrOfPins()          { return _lowLevelArduino.NrOfPins();                          }  /** Returns <code>true</code> if an Arduino is connected and initialized and <code>false</code> otherwise. */
		public Boolean         IsConnected()       { return _lowLevelArduino.IsProperlyInitializedAndConnected(); }  /** returns the associated <code>lowLevelArduino</code> class. Arduino itself is a convenience class. */
		public LowLevelArduino LowLevelArduino()   { return _lowLevelArduino;                                     }
		
		
		public void  ReadUpdate() { _lowLevelArduino.ReadUpdate();  }
		public void WriteUpdate() { _lowLevelArduino.WriteUpdate(); }


		/**
		 * Returns true if a that pin has a specific capability
		 * 
		 * @param pinNr       The pin number of which a pin capability is queried. Analog pins can be queries by their real (high) pin number.
		 * @param capability  The capability as a number. Same as the pinMode setting in SetMode().
		 */
		public Boolean IsCapabilityOfDigitalPin( int pinNr , LowLevelArduino.Firmata.PinModes capability )
		{
			Debug.Assert( pinNr >= 0                          , "pinNr negative, only positive allowed for pinNr ( " + pinNr + ")"                    );
			Debug.Assert( pinNr < _lowLevelArduino.NrOfPins() , "pinNr too high ( " + pinNr + ") , maximum is " + ( _lowLevelArduino.NrOfPins() - 1 ) );

			return _lowLevelArduino.IsCapabilityOfDigitalPin( (uint)pinNr , capability );
		}
		
		
		/**
		 * Returns true if an analog pin has a certain capability
		 * 
		 * @param pinNr       The analog pin number of which a pin capability is queried.
		 * @param capability  The capability as a number. Same as the pinMode setting in SetPinMode().
		 */
		public Boolean IsCapabilityOfAnalogPin( int pinNr , LowLevelArduino.Firmata.PinModes capability )
		{
			Debug.Assert( pinNr >= 0                          , "pinNr negative, only positive allowed for pinNr ( " + pinNr + ")"                    );
			Debug.Assert( pinNr < _lowLevelArduino.NrOfPins() , "pinNr too high ( " + pinNr + ") , maximum is " + ( _lowLevelArduino.NrOfPins() - 1 ) );

			return _lowLevelArduino.IsCapabilityOfAnalogPin( (uint)pinNr , capability );
		}
		
		
		/**
		 * Returns true if a certain pin has a certain capability
		 * 
		 * @param pinNr       The pin of which a pin capability is queried.
		 * @param capability  The capability as a number. Same as the pinMode setting in setPinMode().
		 */
		public Boolean IsCapabilityOfPin( int pinNr , LowLevelArduino.Firmata.PinModes capability )
		{
			Debug.Assert( pinNr >= 0                          , "pinNr negative, only positive allowed for pinNr ( " + pinNr + ")"                    );
			Debug.Assert( pinNr < _lowLevelArduino.NrOfPins() , "pinNr too high ( " + pinNr + ") , maximum is " + ( _lowLevelArduino.NrOfPins() - 1 ) );

			return _lowLevelArduino.IsCapabilityOfPin( (uint)pinNr , capability );
		}
		

		/**
		 * Sets analog pin reporting to a certain mode. See the firmata protocol for details.
		 * 
		 * @param pinNr  The analog pin number.
		 * @param mode   The mode as a number. Same as the pinMode setting in setMode().
		 */
		public void ReportAnalogPin( int pinNr , LowLevelArduino.Firmata.Commands mode )
		{
			Debug.Assert( pinNr >= 0                          , "pinNr negative, only positive allowed for pinNr ( " + pinNr + ")"                    );
			Debug.Assert( pinNr < _lowLevelArduino.NrOfPins() , "pinNr too high ( " + pinNr + ") , maximum is " + ( _lowLevelArduino.NrOfPins() - 1 ) );

			_lowLevelArduino.ReportAnalogPin( (uint)pinNr , mode );
		}
		
		
		/**
		 * Sets digital pin reporting on or off for a certain pin. Analog pins can also be set by their real (high) pin number.
		 * 
		 * @param pinNr  The digital pin number.
		 * @param mode   The mode as a number. Same as the pinMode setting in setMode().
		 */
		public void ReportDigitalPinRange( int pinNr , Boolean isEnabled )
		{
			Debug.Assert( pinNr >= 0                          , "pinNr negative, only positive allowed for pinNr ( " + pinNr + ")"                    );
			Debug.Assert( pinNr < _lowLevelArduino.NrOfPins() , "pinNr too high ( " + pinNr + ") , maximum is " + ( _lowLevelArduino.NrOfPins() - 1 ) );

			_lowLevelArduino.ReportDigitalPinRange( (uint) pinNr / 8 , isEnabled );
		}
		
		
		/**
		 * Sets a specific pin to a certain mode. Analog pins can also be set by their real (high) pin number.
		 * 
		 * @param pinNr    The pin number
		 * @param pinMode  The mode as a number (INPUT/OUTPUT/ANALOG/PWM/SERVO, 0/1/2/3/4) See firmata protocol and Firmata.as for details
		 */
		public void SetPinMode( int pinNr , LowLevelArduino.Firmata.PinModes pinMode )
		{
			Debug.Assert( pinNr >= 0                          , "pinNr negative, only positive allowed for pinNr ( " + pinNr + ")"                    );
			Debug.Assert( pinNr < _lowLevelArduino.NrOfPins() , "pinNr too high ( " + pinNr + ") , maximum is " + ( _lowLevelArduino.NrOfPins() - 1 ) );

			_lowLevelArduino.SetPinMode( (uint)pinNr , pinMode );
		}
		
		
		/**
		 * Sets a pin to servo mode and sets up the servo parameters.
		 * 
		 * @param pinNr     The pin number.
		 * @param angle     The initial angle.
		 * @param minPulse  The minimum pulse. Determines the slowest movements.
		 * @param maxPulse  The initial angle. Determines the fastest movements.
		 */
		public void SetupServo( int pinNr , int angle , int minPulse = 544 , int maxPulse = 2400 )
		{
			Debug.Assert( pinNr >= 0                          , "pinNr negative, only positive allowed for pinNr ( " + pinNr + ")"                    );
			Debug.Assert( pinNr < _lowLevelArduino.NrOfPins() , "pinNr too high ( " + pinNr + ") , maximum is " + ( _lowLevelArduino.NrOfPins() - 1 ) );
			Debug.Assert( angle >=   0                        , "Angle negative, only positive allowed for SetupServo ( " + angle + ")" );
			Debug.Assert( angle <= 180                        , "Angle too large, maximum 180 for SetupServo ( " + angle + ")" );
			Debug.Assert( minPulse >= 0                       , "minPulse negative, only positive allowed for pinNr ( " + minPulse + ")"                    );
			Debug.Assert( maxPulse >= 0                       , "maxPulse negative, only positive allowed for pinNr ( " + maxPulse + ")"                    );

			_lowLevelArduino.SetupServo( (uint)pinNr , (uint)angle , (uint)minPulse , (uint)maxPulse );
		}
		
		
		/**
		 *			Debug.Assert( angle >    0                         , "Angle negative, only positive allowed for PWM (   " + angle + ")" );
			Debug.Assert( angle <= 180                         , "Angle too large, maximum 255 for PWM ( "  + angle + ")" );
 Returns the value of a analog input. Values range between 0 and 1023. The pinNr values range between 0 and 6.
		 *
		 * @param pinNr  Arduino pinNr number of the pinNr to be read. Between 0 and maximum analog pins of the connected Arduino
		 */
		public int ReadAnalog( int pinNr )
		{
			Debug.Assert( pinNr >= 0                          , "pinNr negative, only positive allowed for pinNr ( " + pinNr + ")"                    );
			Debug.Assert( pinNr < _lowLevelArduino.NrOfPins() , "pinNr too high ( " + pinNr + ") , maximum is " + ( _lowLevelArduino.NrOfPins() - 1 ) );

			return (int) _lowLevelArduino.ReadAnalog( (uint)pinNr );
		}
		
		
		/**
		 * Requests and returns the value of a digital input. The pin must be configured as digitalIn. Returns a boolean (true/false).
		 *
		 * @param pinNr  Arduino pinNr number of the pinNr to be read. Between 2 and 13 for normal and between 2-53 for ArduinoMega.
		 */
		public Boolean ReadDigital( int pinNr )
		{
			Debug.Assert( pinNr >= 0                          , "pinNr negative, only positive allowed for pinNr ( " + pinNr + ")"                    );
			Debug.Assert( pinNr < _lowLevelArduino.NrOfPins() , "pinNr too high ( " + pinNr + ") , maximum is " + ( _lowLevelArduino.NrOfPins() - 1 ) );

			return _lowLevelArduino.ReadDigital( (uint) pinNr );
		}
		
		
		/**
		 * Sets an output to a boolean. The pin must be configured as digitalOut. True means ~5V (40 mA) out and false ~0V.
		 *
		 * @param pinNr  Arduino pinNr number of the pinNr to be read. Between 2 and 13 for normal and between 2-53 for ArduinoMega.
		 */
		public void WriteDigital( int pinNr, Boolean isOn )
		{
			Debug.Assert( pinNr >= 0                          , "pinNr negative, only positive allowed for pinNr ( " + pinNr + ")"                    );
			Debug.Assert( pinNr < _lowLevelArduino.NrOfPins() , "pinNr too high ( " + pinNr + ") , maximum is " + ( _lowLevelArduino.NrOfPins() - 1 ) );

			_lowLevelArduino.WriteDigital( (uint) pinNr , isOn );
		}
		
		
		/**
		 * Writes a value (0-255) to a pwm output. The pin must be configured as pwmOut.
		 *
		 * @param pinNr  Arduino pinNr number of the pinNr to be set. Must be 3, 5, 6, 9, 10 or 11. The Arduino mega and others have different configurations.
		 */
		public void WritePWM( int pinNr, int value )
		{
			Debug.Assert( pinNr >= 0                          , "pinNr negative, only positive allowed for pinNr ( " + pinNr + ")" );
			Debug.Assert( pinNr < _lowLevelArduino.NrOfPins() , "pinNr too high ( "  + pinNr + ") , maximum is " + ( _lowLevelArduino.NrOfPins() - 1 ) );
			Debug.Assert( value >= 0                          , "Value negative, only positive allowed for PWM (   " + value + ")" );
			Debug.Assert( value < 256                         , "Value too large, maximum 255 for PWM ( " + value + ")" );

			_lowLevelArduino.WriteAnalogPin( (uint)pinNr, (uint)value );
		}
		
		
		/**
		 * Writes a brightness value to a pwm output. The pin must be configured as pwmOut. The brightness (0-255) is a converted to appear linear for a human.
		 *
		 * @param pinNr  Arduino pinNr number of the pinNr to be set. Must be 3, 5, 6, 9, 10 or 11.
		 */
		public void WriteLed( int pinNr, int brightness )
		{
			Debug.Assert( pinNr >= 0                          , "pinNr negative, only positive allowed for pinNr ( " + pinNr + ")" );
			Debug.Assert( pinNr < _lowLevelArduino.NrOfPins() , "pinNr too high ( "  + pinNr + ") , maximum is " + ( _lowLevelArduino.NrOfPins() - 1 ) );
			Debug.Assert( brightness >= 0                     , "Brightness negative, only positive allowed for WritePWM ( " + brightness + ")" );
			Debug.Assert( brightness < 256                    , "Brightness too large, maximum 255 for WritePWM ( " + brightness + ")" );
		
			_lowLevelArduino.WriteAnalogPin( (uint)pinNr, (uint)_value2led_brightness[brightness] );
		}
		
		/**
		 * Writes a value (0-180) to a servo output. The pin must be configured as servo.
		 *
		 * @param pinNr  Arduino pinNr number of the pinNr to be set. Must be 9 or 10.
		 */
		public void WriteServo( uint pinNr, uint angle )
		{
			Debug.Assert( pinNr >= 0                            , "pinNr negative, only positive allowed for pinNr ( " + pinNr + ")" );
			Debug.Assert( pinNr <   _lowLevelArduino.NrOfPins() , "pinNr too high ( " + pinNr + ") , maximum is " + ( _lowLevelArduino.NrOfPins() - 1 ) );
			Debug.Assert( angle >=  0                           , "Angle negative, only positive allowed for WriteServo ( " + angle + ")" );
			Debug.Assert( angle <= 180                          , "Angle too large, maximum 180 for WriteServo ( " + angle + ")" );

			_lowLevelArduino.WriteAnalogPin( (uint)pinNr, (uint)angle );
		}
	}
}