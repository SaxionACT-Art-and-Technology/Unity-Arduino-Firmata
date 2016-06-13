// ------------------------------------------------------------------------------
// Debug.Logr.cs
//
// Released under MIT license: http://www.opensource.org/licenses/mit-license.php
// Copyright (C) 2015   Douwe A. van Twillert - Art & Technology, Saxion
// ------------------------------------------------------------------------------

using System;
using System.IO;
using System.IO.Ports;
using System.Collections.Generic;
using Utilities;
using UnityEngine;
using Microsoft.CSharp;


namespace OOArduino {
	public partial class LowLevelArduino {
		public partial class Firmata {}
		protected Firmata firmata;

		private uint    _nrOfInitializations = 0;
		private uint    _firmwareVersion     = 0;
		private String  _firmwareVersionName = "";
		private String  _firmwareName        = "";
		private uint    _nrOfPins            = 0;
		private uint    _nrOfDigitalPins     = 0;
		private uint    _nrOfAnalogPins      = 0;
		//private Boolean _wasConnected        = false; 

		private List<List<byte>>       _pinCapabilities  = null;
		private List<Firmata.PinModes> _currentPinMode   = new List<Firmata.PinModes>();
		private string[]               _pinConfiguration = { "", "" };
		
		private uint[] _analogInputData   = new uint[ (int)Firmata.ProtocolDetails.MAX_NR_OF_PINS        ];
		private uint[] _analogOutputData  = new uint[ (int)Firmata.ProtocolDetails.MAX_NR_OF_ANALOG_PINS ];
		private byte[] _pwmOutputData     = new byte[ (int)Firmata.ProtocolDetails.MAX_NR_OF_PINS        ];
		private byte[] _digitalInputData  = new byte[ (int)Firmata.ProtocolDetails.MAX_CHANNELS          ];
		private byte[] _digitalOutputData = new byte[ (int)Firmata.ProtocolDetails.MAX_CHANNELS          ];


		public LowLevelArduino( string[] pinConfiguration , String portName, int baudRate )
		{
			firmata = new Firmata( this, portName , baudRate );
			_pinConfiguration = pinConfiguration;
		}

		public void ReadUpdate()
		{
			firmata.ReadUpdate();
			if ( !IsConnected()) {
				_firmwareVersion = 0;
				_pinCapabilities = null;
			}
		}

		public void WriteUpdate()
		{
			firmata.WriteUpdate();
			if ( !IsConnected()) {
				_firmwareVersion = 0;
				_pinCapabilities = null;
			}
		}

		// Getters and setters

		public Boolean IsMega()                            { return _nrOfDigitalPins == 54;                          }  /** Returns <code>true</code> if the connected Arduino is an Mega arduino. */ 
		public uint    FirmwareVersion()                   { return _firmwareVersion;                                }  /** Returns the firmware version in a single number (2.3 translates to 23). Can be 0 .*/
		public String  FirmwareVersionString()             { return _firmwareVersionName;                            }  /** Returns the firmware version as a string. Can be <code>null</code>. */
		public String  FirmwareName()                      { return _firmwareName;                                   }  /** Returns the name of the firmware, can be <code>null</code> */
		public uint    NrOfDigitalPins()                   { return _nrOfDigitalPins;                                }  /** Returns number of digital pins. */
		public uint    NrOfAnalogPins()                    { return _nrOfAnalogPins;                                 }  /** Returns number of analog pins. */
		public uint    NrOfPins()                          { return _nrOfDigitalPins + _nrOfAnalogPins;              }  /** Returns the total number of pins. Is the same as the sum of the number of analog and digital pins. */
		public Boolean IsConnected()                       { return firmata.IsConnected();                           }  /** Returns <code>true</code> if an Arduino is connected and <code>false</code> otherwise. */
		public Boolean IsProperlyInitializedAndConnected() { return _pinCapabilities != null && _currentPinMode.Count == _pinCapabilities.Count; }  /** Returns <code>true</code> if an Arduino is connected and initialized and <code>false</code> otherwise. */


		/*
		 * Returns true if a certain pin has a certain capability.
		 * 
		 * @param pinNr       The pin number of which a pin capability is queried. Analog pins can also be queries by their real (high) pin number.
		 * @param capability  The capability as a number. Same as the pinMode setting in setMode().
		 */
		public Boolean IsCapabilityOfDigitalPin( uint pin , Firmata.PinModes capability )
		{
			Debug.Assert( pin < _nrOfDigitalPins , "digital pin (" + pin + ") must be less than maximum (" + _nrOfDigitalPins + ")" );
			
			return IsCapabilityOfPin( pin, capability );
		}
		
		
		/*
		 * Returns true if an analog pin has a specific capability.
		 * 
		 * @param pinNr       the analog pin number of which a pin capability is queried.
		 * @param capability  the capability as a number. Same as the pinMode setting in setPinMode().
		 */
		public Boolean IsCapabilityOfAnalogPin( uint pin , Firmata.PinModes capability )
		{
			Debug.Assert( pin < _nrOfAnalogPins , "analog pin (" + pin + ") must be less than maximum (" + _nrOfAnalogPins + ")"  );
			
			return IsCapabilityOfPin( pin + _nrOfDigitalPins, capability );
		}
		
		
		/*
		 * Returns true if a certain pin has a specific capability.
		 * 
		 * @param pinNr       the pin of which a pin capability is queried.
		 * @param capability  the capability as a number. Same as the pinMode setting in setPinMode().
		 */
		public Boolean IsCapabilityOfPin( uint pin , Firmata.PinModes capability ) {
			Debug.Assert( (int)pin < _nrOfPins, "pin (" + pin + ") must be less than maximum (" + _nrOfPins + ")" );
			Debug.Assert( (int)capability < (int)Firmata.TOTAL_PIN_MODES , "capability (" + capability + ") must be less than maximum (" + Firmata.TOTAL_PIN_MODES + ")" );
			
			return _pinCapabilities[ (int)pin ] [ (int)capability ] != 0;
		}


		/*
		 * Sets analog pin reporting to a certain mode. See the firmata protocol for details.
		 * 
		 * @param pinNr  The analog pin number.
		 * @param mode   The mode as a number. Same as the pinMode setting in setMode().
		 */
		public void ReportAnalogPin( uint pin , Firmata.Commands mode )
		{
			Debug.AssertFormat(  pin <  16 || pin >= _nrOfDigitalPins, "pin (" + pin + ") must be between 0 and 15 or real analog pin mapping" , pin );
			Debug.AssertFormat( (int)mode < 256 , "mode  (" + mode + ") for pin  (" + pin + ") must be between 0 and 255" , mode, pin );
			
			if ( IsProperlyInitializedAndConnected() ) {
				firmata.ReportAnalogPin( pin, mode );
			}
		}
		
		
		/*
		 * Sets digital pin reporting on or off for a certain pin. Analog pins can also be set by their real (high) pin number.
		 * 
		 * @param pinNr  The digital pin number.
		 * @param mode   The mode as a number. Same as the pinMode setting in setMode().
		 */
		public void ReportDigitalPinRange( uint port , Boolean isEnabled ) 
		{
			Debug.Assert( port < _nrOfDigitalPins , "port  (" + port + ") must be between 0 and maximum  (" + Firmata.ProtocolDetails.MAX_ANALOG_PIN + ")" );
			
			if ( IsProperlyInitializedAndConnected() ) {
				firmata.ReportDigitalPort( port, isEnabled );
			}
		}
		
		
		/*
		 * Sets a specific pin to a certain mode. Analog pins can also be set by their real (high) pin number.
		 * 
		 * @param pinNr    The pin number.
		 * @param pinMode  The mode as a number (INPUT/OUTPUT/ANALOG/PWM/SERVO, 0/1/2/3/4) See firmata protocol and Firmata.as for details.
		 */
		public void SetPinMode( uint pinNr , Firmata.PinModes pinMode  ) {
			Debug.Assert(        pinNr < _nrOfPins               , "pin  ("  + pinNr   + ") must be between 0 and total number of pins  (" + _nrOfPins               + ")" );
			Debug.Assert( (int)pinMode < Firmata.TOTAL_PIN_MODES , "mode  (" + pinMode + ") must be between 0 and maximum pin mode  ("     + Firmata.TOTAL_PIN_MODES + ")" );
			
			if ( IsProperlyInitializedAndConnected() ) {
				if ( pinMode == Firmata.PinModes.SERVO ) {
					firmata.SetupServo    ( pinNr , 0 );
					firmata.WriteAnalogPin( pinNr , 0 ); // write set start position to 0 otherwise it turns directly to 90 degrees.
				} else {
					firmata.SetPinMode( pinNr , pinMode );
				}
			}
			 _currentPinMode[ (int)pinNr ] = pinMode;
			if ( pinNr >= _nrOfDigitalPins && pinMode == Firmata.PinModes.ANALOG ) {
				firmata.ReportAnalogPin( pinNr - _nrOfDigitalPins, Firmata.Commands.ENABLE );
			}
		}
		
		
		/*
		 * Sets a pin to servo mode and sets up the servo parameters.
		 * 
		 * @param pinNr     The pin number.
		 * @param angle     The initial angle.
		 * @param minPulse  The minimum pulse. Determines the slowest movements.
		 * @param maxPulse  The initial angle. Determines the fastest movements.
		 */
		public void SetupServo( uint pin , uint angle , uint minPulse = 544 , uint maxPulse = 2400 )
		{
			Debug.Assert( pin < _nrOfDigitalPins , "pin (" + pin + ") must be between 0 and max (" + _nrOfDigitalPins + ")" );
			Debug.Assert( _currentPinMode[ (int)pin ] == Firmata.PinModes.SERVO , "pin (" + pin + ") must be configured as servo but was  (" + _currentPinMode[ (int)pin ] + ")" );
			
			if ( IsProperlyInitializedAndConnected() ) {
				// TODO, remember angle, minPulse and maxPulse
				firmata.SetupServo( pin , angle , minPulse , maxPulse );
			}
		}

		
		/*
		 * Writes a value (0-255) to an PWM or servo output. The pin must be configured as PWM or SERVO.
		 *
		 * @param pinNr  Arduino pinNr number of the pinNr to be set. Must be 3, 5, 6, 9, 10 or 11 for an DueMilaNove or Uno. The Arduino mega and others have different configurations.
		 */
		public void WriteAnalogPin( uint pinNr , uint value )
		{
			Debug.Assert( pinNr <= _nrOfPins, "pin (" + pinNr + ") must be between 0 and max (" + _nrOfPins + ")" );
			Debug.Assert(  _currentPinMode[ (int)pinNr ] == Firmata.PinModes.PWM ||  _currentPinMode[ (int)pinNr ] == Firmata.PinModes.SERVO ,
			              "pin  (" + pinNr + ") must be configured as pwm or servo but was  (" + (int)  _currentPinMode[ (int)pinNr ]
			             + "=" + Firmata.Capability2string(  _currentPinMode[ (int)pinNr ] ) + ")" );
			
			if ( IsProperlyInitializedAndConnected() ) {
				// TODO, add timed writes (sometimes necessary)
				if ( _analogOutputData[ pinNr ] != value ) {
					_analogOutputData[ pinNr ] = value;
					//trace( "write analog value " + value );
					if ( pinNr < (uint) Firmata.ProtocolDetails.MAX_ANALOG_PIN ) {
						firmata.WriteAnalogPin        ( pinNr, value );
					} else {
						firmata.ExtendedWriteAnalogPin( pinNr, value );
					}
				}
			}
		}
		
		/*
		 * Returns the value of a analog input. Values range between 0 and 1023. The pin values range between 0 and 6.
		 *
		 * @param pin  Arduino pin number of the pin to be read. Between 0 and 6
		 */
		public uint ReadAnalog( uint pin )
		{
			Debug.Assert( pin < _nrOfAnalogPins , "pin  (" + pin + ") larger than possible for this Arduino  (" + ( _nrOfAnalogPins - 1 ) + ")" );
			Debug.Assert( _currentPinMode[ (int) AnalogPinToRealPin( pin ) ] == Firmata.PinModes.ANALOG, "pin (" + pin + ") must be configured as analog" );

			return _analogInputData[ pin ];
		}
		
		/*
		 * Requests and returns the value of a digital input. Must be configured as digitalIn. Returns a boolean (true/false).
		 *
		 * @param pin  Arduino pin number of the pin to be read. Between 2 and 13 for normal and between 2-53 for ArduinoMega.
		 */
		public Boolean ReadDigital( uint pin )
		{
			Debug.Assert( pin <= _nrOfPins, "pin (" + pin + ") must be between 0 and max (" + _nrOfPins + ")" );
			Debug.Assert( _currentPinMode[ (int)pin ] == Firmata.PinModes.INPUT, "pin (" + pin + ") must be configured as input" );

			uint channel = pin >> 3 ;
			uint mask    = (uint) ( 1 << ( (int) pin % 8 ) );
			
			return ( _digitalInputData[ channel ] & mask ) != 0 ? true : false ;
		}
		
		/*
		 * Sets an output to a boolean. Must be configured as digitalOut. True means ~5V (40 mA) out and false ~0V.
		 *
		 * @param pin  Arduino pin number of the pin to be read. Between 2 and 13 for normal and between 2-53 for ArduinoMega.
		 */
		public void WriteDigital( uint pin , Boolean isOn )
		{
			Debug.Assert( pin <= _nrOfPins, "pin (" + pin + ") must be between [ 0 , max > (" + _nrOfPins + ")" );
			Debug.Assert( _currentPinMode[ (int)pin ] == Firmata.PinModes.OUTPUT, "pin (" + pin + ") must be configured as output" );

			int channel          = (int) pin >> 3 ;
			int mask             =   1 << ( (int)pin % 8 ) ;
			byte oldChannelValue = _digitalOutputData[ channel ];

			if ( isOn ) {
				_digitalOutputData[ channel ] = (byte) ( _digitalOutputData[ channel ] |  mask );
			} else {
				_digitalOutputData[ channel ] = (byte) ( _digitalOutputData[ channel ] & ~mask );
			}
			
			if ( IsProperlyInitializedAndConnected() && oldChannelValue != _digitalOutputData[ channel ] ) {
				firmata.WriteDigitalPins( (byte)channel , _digitalOutputData[ channel ] );
			}
		}
		
		
		/*
		 * Writes a value (0-255) to a pwm output. Must be configured as pwmOut.
		 *
		 * @param pin  Arduino pin number of the pin to be set. Must be 3, 5, 6, 9, 10 or 11.
		 */
		public void WriteAnalog( uint pin , uint value  )
		{
			Debug.Assert( value < 256 , "Analog write values must be smaller than 256 but was " + value );

			CheckArduinoInitAndPinConfig( pin , _currentPinMode[ (int)pin ] );
			if ( IsProperlyInitializedAndConnected() ) {
				firmata.WriteAnalogPin( pin, value );
			}
			_pwmOutputData[ pin ] = (byte)value;
		}
		
		
		/*
		 * Sends a system reset request to the arduino.
		 */
		public void SystemReset()
		{
			firmata.SystemReset();
		}
		
		
		/**
		 * Sends an I2C write request to the Arduino.
		 *
		 * @param slaveAddress	the address which was set for the I2C device 
		 * @param data			the data to be writte nto the I2C device
		 */
		public void SendI2CwriteRequest( uint slaveAddress , List<byte> data )
		{
			firmata.SendI2CwriteRequest( slaveAddress , data );
		}
		
		
		/*
		 * Sends an I2C write request to the Arduino.
		 *
		 * @param slaveAddress		the address which was set for the I2C device 
		 * @param numberOfBytes		the number of bytes to be read
		 */
		public void SendI2CreadOnceRequest( uint slaveAddress , uint numberOfBytes )
		{
			firmata.SendI2CreadOnceRequest( slaveAddress, numberOfBytes );
		}
		
		
		public void SendI2CreadContiniouslyRequest( uint slaveAddress )
		{
			firmata.SendI2CreadContiniouslyRequest( slaveAddress );
		}
		
		
		public void SendI2CstopReadingRequest( uint slaveAddress )
		{
			firmata.SendI2CstopReadingRequest( slaveAddress );
		}
		
		
		public void SendI2Cconfig( Boolean powerPinSetting , uint delay )
		{
			firmata.SendI2Cconfig( powerPinSetting , delay );
		}
		
		
		public void SetSamplingInterval( uint intervalInMilliseconds )
		{
			firmata.SetSamplingInterval( intervalInMilliseconds );
		}
		

		// =========================================
		// Functions used by the firmata reader part
		// =========================================
		private void Analog_IO_MessageReceived( byte pin , uint newValue )
		{
			uint oldValue = _analogInputData[ pin ];
			_analogInputData[ pin ] = newValue;
			if ( oldValue != newValue && IsProperlyInitializedAndConnected()  ) {
				// TODO dispatchEvent( new NewDataEvent( NewDataEvent.NEW_ANALOG_DATA , pin , newValue ) );
			}
		}
		
		
		public void Digital_IO_MessageReceived( byte channel, int newChannelValue )
		{
			if ( IsProperlyInitializedAndConnected() && _digitalInputData[channel] != newChannelValue ) {
					// TODO if ( hasEventListener( NewDataEvent.NEW_DIGITAL_DATA ) ) {
						//dispatchEvent( new NewDataEvent( NewDataEvent.NEW_DIGITAL_DATA , channel * 8 + channelPin , newValue & mask ? 1 : 0 ) );
					//}
				_digitalInputData[channel] = (byte)  newChannelValue;
			}
		}
		
		
		public void QueryFirmwareReceived( uint majorVersion, uint minorVersion  )
		{
			if ( _firmwareVersion == 0 ) {
				firmata.RequestCapabilities();
			}
			_firmwareVersion     = majorVersion * 10  + minorVersion;
			_firmwareVersionName = majorVersion + "." + minorVersion;

			Debug.Assert( _firmwareVersion >= 20 , "Firmware version , too low for this software, at least 2.0 expected" );
		}
		
		
		public void QueryFirmwareAndNameReceived( uint majorVersion , uint minorVersion , String name )
		{
			QueryFirmwareReceived( majorVersion, minorVersion );
			_firmwareName = name;
			Debug.Log ( "Firmware version name '" + name + "'" );
		}
		
		
		public void I2CReplyReceived( uint address , uint register , List<int> data )
		{
			//if ( hasEventListener( I2CDataEvent.I2C_DATA_MESSAGE ) ) {
				// TODO dispatchEvent( new I2CDataEvent( I2CDataEvent.I2C_DATA_MESSAGE , Firmata.I2C_REPLY , address , register , data ) );
			//}
		}
		
		
		public void SysexStringReceived( Firmata.Commands command , String message )
		{
			Debug.Log( "Sysex string received: '"  + message + "'" );
			//if ( hasEventListener( SysexEvent.SYSEX_STRING_MESSAGE ) ) {
				//TODO dispatchEvent( new SysexEvent( SysexEvent.SYSEX_STRING_MESSAGE , command , message , null ) );
			//}
		}
		
		
		public void SysexDataReceived( byte command , List<int> data )
		{
			Debug.Log( "Sysex data received: data=" + Tracer.traceArray( data ) );
			//if ( hasEventListener( SysexEvent.SYSEX_DATA_MESSAGE ) ) {
				//TODO dispatchEvent( new SysexEvent( SysexEvent.SYSEX_DATA_MESSAGE , command , "" , data ) );
			//}
		}
		
		
		public void UnknownCommandReceived( uint command )
		{
			Debug.Log( "unknown command received: command="  + command.ToString("X2") );
			// TODO dispatchEvent( new UnknownCommandEvent( UnknownSysexCommandEvent.ARDUINO_NEW_ANALOG_DATA , command , message ) );
		}
		
		
		public void UnknownSysexCommandReceived( uint command , List<byte> data )
		{
			Debug.Log( "unknown sysex command received: command=" + command + " data='"  + Tracer.Bytes2Array( data) + "'" );
			// TODO dispatchEvent( new UnknownSysexCommandEvent( UnknownSysexCommandEvent.ARDUINO_NEW_ANALOG_DATA , command , message ) );
		}
		
		
		public void PinCapabilitiesReceived( uint nrOfAnalogPins , uint nrOfDigitalPins , List<List<byte>> pinCapabilities )
		{
			if ( IsFirmwareReceived() ) {
				_nrOfPins        = nrOfDigitalPins + nrOfAnalogPins;
				_nrOfAnalogPins  = nrOfAnalogPins;
				_nrOfDigitalPins = nrOfDigitalPins;
				_pinCapabilities = pinCapabilities;

				Initialize();
				if ( _firmwareVersion > 21 ) {
					for ( uint i = 2 ; i < _nrOfPins ; i++ ) {
						firmata.RequestPinState( i );
					}
				}
			}
		}
		

		public void PinStateResponseReceived( uint pin , Firmata.PinModes state , uint value )
		{
			if ( _firmwareVersion > 0 ) {
				if ( pin >= _pinConfiguration.Length && ( pin < _nrOfDigitalPins + _nrOfAnalogPins ) ) {
					_currentPinMode[ (int)pin ] = state;
					// FIXME, value always seems to be 0, so don't overwrite previously stored values
					TracePinConfig( pin, state );
				}
				//TODO if ( hasEventListener( PinStateEvent.PIN_STATE_RECEIVED ) ) {
				//	dispatchEvent( new PinStateEvent( PinStateEvent.PIN_STATE_RECEIVED , pin , state , value ) );
				//}
			}
		}

		
		private Boolean IsFirmwareReceived()
		{
			return _firmwareVersion != 0;
		}

		protected void Initialize()
		{
			Debug.Assert( IsFirmwareReceived() , "Oops, initializing, while still waiting for firmware response??" );
			
			Debug.Log( "[Connected to Arduino with Firmata version: " + _firmwareVersionName + "]" );
			
			//if ( _currentPinMode.Length == _nrOfPins ) {
			//	SetRememberedPinModes();
			//} else {
				AssertAndAssignArduinoPinConfiguration();
			//}
			TurnOnAnalogAndDigitalPinReporting();
			
			// TODO if ( hasEventListener( ArduinoEvent.INITIALIZED ) ) {
			//	dispatchEvent( new ArduinoEvent( ArduinoEvent.INITIALIZED, ++_nrOfInitializations ) );
			//}
		}

		

		private void AssertAndAssignArduinoPinConfiguration()
		{
			Debug.Assert( IsFirmwareReceived()                  , "Oops, initializing, while still waiting for firmware response??" );
			Debug.Assert( _pinConfiguration.Length <= _nrOfPins , "pin (" + _pinConfiguration.Length + ") configuration too long for this Arduino (" + _nrOfPins + ")" );

			string pinConfiguration = "Arduino pin configuration:" + _pinConfiguration.Length;
			//Debug.Log ( "STARTING PIN CONFIG" );
			_currentPinMode.Clear();
			for ( uint pin = 0 ; pin < _pinConfiguration.Length ; pin++ ) {
				//Debug.Log ( "PINCONFIG = " + pinConfiguration );
				Firmata.PinModes pinMode = Firmata.PinModes.ILLEGAL_PIN_MODE;
				string capabilities = PinCapabilities2String( pin );
				//Debug.Log ( "Capabilities[" + pin + "] = " + capabilities );
				if ( capabilities.Length == 0 ) {
					Debug.Assert( _pinConfiguration[ pin ] == null || _pinConfiguration[ pin ].Length == 0 , "pin (" + pin + ") has no capabilities (" + _pinConfiguration[ pin ] + "), use null or empty string" );
				} else {
					pinMode = Firmata.PinConfigurationString2PinMode( _pinConfiguration[ pin ] );
					Debug.Assert( IsCapabilityOfPin( pin , pinMode ), "pin " + pin + " is not properly configured (" + _pinConfiguration[ pin ] + ")" );
				}
				_currentPinMode.Add( pinMode );
				pinConfiguration = pinConfiguration + "\n" + TracePinConfig( pin , pinMode );
			}

			for ( uint pin = (uint)_pinConfiguration.Length ; pin < _nrOfPins ; pin++ ) {
				Firmata.PinModes pinMode = Firmata.PinModes.ILLEGAL_PIN_MODE;
				if ( IsCapabilityOfPin( pin , Firmata.PinModes.ANALOG ) ) {
					pinMode = Firmata.PinModes.ANALOG;
				} else if ( IsCapabilityOfPin( pin , Firmata.PinModes.INPUT ) ) {
					pinMode = Firmata.PinModes.INPUT;
				}
				_currentPinMode.Add( pinMode );
				pinConfiguration = pinConfiguration + "\n    <no config> -> " + TracePinConfig( pin , pinMode );
			}
			SetRememberedPinModes();
			Debug.Log( pinConfiguration );
			_nrOfInitializations++;
			//_wasConnected = true;
		}
		
	
		private String TracePinConfig( uint pin, Firmata.PinModes currentState )
		{
			String capabilities = PinCapabilities2String( pin );
			if ( capabilities != "" ) {
				capabilities = "\t(possible modes=" + capabilities + ")";
			}
			/*
			if ( IsCapabilityOfPin( pin, Firmata.PinModes.ANALOG ) )
				capabilities += "\t(currentValue = " +  _analogInputData[ RealPinToAnalogPin( pin ) ] + ")";
			else
				capabilities += "\t(currentValue = " + _digitalInputData[ pin                       ] + ")";
				*/
			
			return "    pin " + pin + " -> " + Firmata.Capability2string( currentState ) + "\t" + capabilities;
		}
		
		
		private String PinCapabilities2String( uint pin )
		{
			String capabilities = "";

			//Debug.Log( "Pin " + pin + " = " + Tracer.traceArray(  _pinCapabilities[ (int)pin ] ) );
			
			for ( Firmata.PinModes capability = Firmata.PinModes.INPUT ; (int)capability < Firmata.TOTAL_PIN_MODES ; capability++ ) {
				if ( ( pin < _nrOfDigitalPins + _nrOfAnalogPins ) && _pinCapabilities[ (int)pin ][ (int)capability ] != 0 ) {
					capabilities = capabilities + ( capabilities.Length == 0 ? "" : ", " ) + Firmata.Capability2string( capability );
					if ( _pinCapabilities[ (int)pin ][ (int)capability ] > 0 ) {
						capabilities = capabilities + "(" + _pinCapabilities[ (int)pin ][ (int)capability ] + ")";
					}
				}
			}

			return capabilities;
		}
		
		
		private void SetRememberedPinModes()
		{
			for ( int i = 0 ; i < _currentPinMode.Count ; i++ )	{
				if ( _currentPinMode[ i ] != Firmata.PinModes.ILLEGAL_PIN_MODE ) {
					firmata.SetPinMode( (uint)i, _currentPinMode[ i ] );
				}
			}
		}


		private void CheckArduinoInitAndPinConfig( uint pin , Firmata.PinModes pinMode )
		{
			Debug.Assert( _pinCapabilities != null          , "Arduino is not (yet) initialized or disconnected, use ArduinoEvent" );
			Debug.Assert( pin < _nrOfDigitalPins            , "pin (" + pin + ") larger than possible for this Arduino (" + _nrOfDigitalPins + ")" );
			Debug.Assert( _currentPinMode[ (int)pin ] == pinMode , "pin (" + pin + ") not configured for (" + pinMode + ") but for (" + _currentPinMode + ")" );
		}
		
		
		private void TurnOnAnalogAndDigitalPinReporting()
		{
			Debug.Assert( IsProperlyInitializedAndConnected() , "Oops, already initializing while still waiting for firmware response??" );
			
			for ( uint pin = 0 ; pin < _nrOfAnalogPins ; pin++ ) {
				firmata.ReportAnalogPin( pin, Firmata.Commands.ENABLE );
			}

			for( int port = (int) AnalogPinToRealPin( ( _nrOfAnalogPins - 1 ) ) / 8 ; port >= 0 ; port-- ) {
				firmata.ReportDigitalPort( (uint) port, true );
			}
		}
		
		protected uint AnalogPinToRealPin( uint analogPin )
		{
			Debug.Assert( _nrOfAnalogPins > analogPin , "analogPin (" + analogPin + ") too large for nrOfAnalogPins(" + _nrOfAnalogPins + ")" );
			Debug.Assert( _nrOfDigitalPins > 0 , "no digital pins" );

			return _nrOfDigitalPins + analogPin;
		}
		
		protected uint RealPinToAnalogPin( uint realPin )
		{
			Debug.Assert( realPin <= _nrOfPins        , "realPin (" + realPin + ") must be between " + _nrOfDigitalPins + " and max (" + _nrOfPins + ")" );
			Debug.Assert( realPin >= _nrOfDigitalPins , "realPin (" + realPin + ") ) too small for nrOfDigitalPins(" + _nrOfDigitalPins + ")"           );

			return realPin - _nrOfDigitalPins;
		}
	}
}
