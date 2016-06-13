// ------------------------------------------------------------------------------
// Firmata.cs
//
// Released under MIT license: http://www.opensource.org/licenses/mit-license.php
// Copyright (C) 2015   Douwe A. van Twillert - Art & Technology, Saxion
// ------------------------------------------------------------------------------
using System;
using System.IO;
using System.IO.Ports;

using UnityEngine;


namespace OOArduino {

	public partial class LowLevelArduino {
		public partial class Firmata
		{
			private LowLevelArduino _parent;
			private      SerialPort _serial;
			private             int lastTimeTriedToOpenOrOpen = -2000;

			public Firmata( LowLevelArduino parent, String portName, int baudRate ) {
				_parent = parent;
				_serial = new SerialPort( portName , baudRate );
				_serial.ReadTimeout  = 50;
				_serial.WriteTimeout = 50;
				OpenSerialPortIfNecessary();
			}

			public enum Commands : byte 
			{
				// firmata messages, details from http://firmata.org/wiki/V2.2ProtocolDetails
				// names and comments taken from Firmata.h
				DIGITAL_MESSAGE               = 0x90, // send data for a digital pin

				REPORT_ANALOG                 = 0xC0, // enable analog input by pin #
				REPORT_DIGITAL                = 0xD0, // enable digital input by port pair
				ANALOG_MESSAGE                = 0xE0, // send data for an analog pin (or PWM)

				SET_PIN_MODE                  = 0xF4, // set a pin to INPUT/OUTPUT/PWM/etc
				
				REPORT_VERSION                = 0xF9, // report protocol version
				SYSTEM_RESET                  = 0xFF, // reset from MIDI
				
				SYSEX_START		              = 0xF0, // start a MIDI Sysex message
				SYSEX_END		              = 0xF7, // end a MIDI Sysex message
				
				// sysex commands
				ANALOG_MAPPING_QUERY          = 0x69, // ask for mapping of analog to pin numbers
				ANALOG_MAPPING_RESPONSE       = 0x6A, // reply with mapping info
				CAPABILITY_QUERY              = 0x6B, // ask for supported modes and resolution of all pins
				CAPABILITY_RESPONSE           = 0x6C, // reply with supported modes and resolution
				PIN_STATE_QUERY               = 0x6D, // ask for a pin's current mode and value
				PIN_STATE_RESPONSE            = 0x6E, // reply with pin's current mode and value
				EXTENDED_ANALOG               = 0x6F, // analog write (PWM, Servo, etc) to any pin
				SERVO_CONFIG                  = 0x70, // set max angle, minPulse, maxPulse, freq
				STRING_DATA                   = 0x71, // a string message with 14-bits per char
				SHIFT_DATA                    = 0x75, // a bitstream to/from a shift register
				I2C_REQUEST                   = 0x76, // send an I2C read/write request
				I2C_REPLY                     = 0x77, // a reply to an I2C read request
				I2C_CONFIG                    = 0x78, // config I2C settings such as delay times and power pins
				REPORT_FIRMWARE               = 0x79, // report name and version of the firmware
				SAMPLING_INTERVAL             = 0x7A, // set the poll rate of the main loop
				SYSEX_NON_REALTIME            = 0x7E, // MIDI Reserved for non-realtime messages
				SYSEX_REALTIME                = 0x7F, // MIDI Reserved for realtime messages
				
				ENABLE                        = 1,
				DISABLE                       = 0,

				// IC2 constants
				I2C_10_BITS_ADDRESS_MODE      =  1 << 5,
				I2C_REQUEST_WRITE             = 00 << 3,
				I2C_REQUEST_READ_ONCE         = 01 << 3,
				I2C_REQUEST_READ_CONTINUOUSLY = 10 << 3,
				I2C_REQUEST_STOP_READING      = 11 << 3
			};

			public enum ProtocolDetails : int {
				// protocol details, depending on how many bits/bytes are used for pins, ports etc.
				MAX_CHANNELS                  = 1 <<  3,
				MAX_NR_OF_PINS                = 1 <<  8,
				MAX_ANALOG_DATA               = 1 << 14,  // TODO, use the retrieved precision
				MAX_ANALOG_PIN                = 15,
				MAX_NR_OF_ANALOG_PINS         = 16
			};

				
			public enum PinModes : byte {
				ILLEGAL_PIN_MODE              = 0xFF, // Illegal pin mode
				INPUT                         = 0x00, // pin in Input mode
				OUTPUT                        = 0x01, // pin in Output mode
				ANALOG                        = 0x02, // analog pin in analogInput mode
				PWM                           = 0x03, // digital pin in PWM output mode
				SERVO                         = 0x04, // digital pin in Servo output mode
				SHIFT                         = 0x05, // shiftIn/shiftOut mode
				I2C                           = 0x06  // pin included in I2C setup
			};

			public const uint TOTAL_PIN_MODES = (uint) PinModes.I2C + 1;
				

			// ================
			// Public functions
			// ================
			public static PinModes PinConfigurationString2PinMode( String type )
			{
				switch( type ) {
					case "digitalIn"  : return PinModes.INPUT  ;
					case "analogIn"   : return PinModes.ANALOG ;
					case "digitalOut" : return PinModes.OUTPUT ;
					case "pwmOut"     : return PinModes.PWM    ;
					case "servo"      : return PinModes.SERVO  ;
					case "i2c"        : return PinModes.I2C    ;
				}
				return PinModes.ILLEGAL_PIN_MODE;
			}
			
			public static String Capability2string( PinModes capabilityNr )
			{
				switch( (PinModes) capabilityNr ) {
					case PinModes.INPUT            : return "digitalIn"  ;
					case PinModes.ANALOG           : return "analogIn"   ;
					case PinModes.OUTPUT           : return "digitalOut" ;
					case PinModes.PWM              : return "pwmOut"     ;
					case PinModes.SERVO            : return "servo"      ;
					case PinModes.SHIFT            : return "shift"      ;
					case PinModes.I2C              : return "i2c"        ;
					case PinModes.ILLEGAL_PIN_MODE : return "<illegal pin mode>";
				}
				return "<no defined capability for (" + capabilityNr + ")>";
			}

			private void OpenSerialPortIfNecessary()
			{

				int now = UnityEngine.Time.frameCount;
				//Debug.Log( "OpenSerialPortIfNecessary(): now=" + UnityEngine.Time.frameCount );
				if ( ! _serial.IsOpen ) {
					//Debug.Log( "OpenSerialPortIfNecessary(): diff=" + ( now - lastTimeTriedToOpenOrOpen ) );
					if ( now - lastTimeTriedToOpenOrOpen > 300 ) {
						Debug.Log ( "OpenSerialPortIfNecessary(): Trying to open Serial Port" );
						_readBuffer    = null;
						_nrOfBytes     = 0;
						try {
							_serial.Open();
						} catch ( Exception exception ) {
							Debug.Log ( "exception caught: "  + exception );
						}
						_serial.ReadTimeout  = 50;
						_serial.WriteTimeout = 50;
						lastTimeTriedToOpenOrOpen = now;
						RequestReportVersion();
					}
				} else {
					lastTimeTriedToOpenOrOpen = now;
				}
				lastTimeTriedToOpenOrOpen = now;
			}
		}
	}
}

