// ------------------------------------------------------------------------------
// FirmataSender.cs
//
// Released under MIT license: http://www.opensource.org/licenses/mit-license.php
// Copyright (C) 2015   Douwe A. van Twillert - Art & Technology, Saxion
// ------------------------------------------------------------------------------

using System;
using System.IO;
using System.IO.Ports;
using System.Collections.Generic;

using UnityEngine;

namespace OOArduino
{
	public partial class LowLevelArduino 
	{
		public partial class Firmata 
		{
			#region FirmateWriter

			private Byte[] _writeBuffer = new Byte[8192];
			private int    _nrOfBytes   = 0;

			// getters
			public String  port()         { return _serial.PortName; }
			public Boolean IsConnected()  { return _serial.IsOpen;   }

			public void WriteUpdate()
			{
				// TODO how to write info messages and filter them
				//if ( _nrOfBytes > 0 ) { Debug.Log ( "Writing " + _nrOfBytes + " bytes" ); }
				WriteBuffer();
			}

			//---------------------------------------
			// toggle analogIn reporting by pin
			// 0  toggle digitalIn reporting (0xC0-0xCF) (MIDI Program Change)
			// 1  disable(0)/enable(non-zero)
			//---------------------------------------
		public void ReportAnalogPin( uint pin , Commands mode )
			{
				Debug.Assert(      pin  < 16 );
				Debug.Assert( (int)mode <  2 );
				
				WriteByte( (byte) Commands.REPORT_ANALOG + ( pin & 15 ) );
				WriteByte( (byte) mode                                  );
				//Flush();
			}
			
			
			//--------------------------------------------------------------
			// TODO FIX WRONG COMMENT
			// 1  port number, between 1 and 15 (is pin number divided by 8)
			// 2  isEnabled, 
			//--------------------------------------------------------------
			public void ReportDigitalPort( uint port , Boolean isEnabled )
			{
				Debug.Assert( port < 16 );

				WriteByte( (byte) Commands.REPORT_DIGITAL + ( port & 15 )            );
				WriteByte( (byte) ( isEnabled ? Commands.ENABLE : Commands.DISABLE ) );
				//Flush();
			}
			
			
			//----------------------------------------------------
			// 1  set digital pin mode (0xF4) (MIDI Undefined)
			// 2  pin number (0-127)
			// 3  state (INPUT/OUTPUT/ANALOG/PWM/SERVO, 0/1/2/3/4)
			//----------------------------------------------------
			public void SetPinMode( uint pin , PinModes mode )
			{
				Debug.Assert( pin < 128 );

				WriteByte( (byte) Commands.SET_PIN_MODE );
				WriteByte( (byte) ( pin & 127 )         );
				WriteByte( (byte) mode                  );
				//Flush();
			}
			
			
			public void SetupServo( uint pin , uint angle , uint minPulse = 544 , uint maxPulse = 2400 )
			{
				Debug.Assert( pin < 16 );
				
				WriteByte( (byte) Commands.SYSEX_START  );
				WriteByte( (byte) Commands.SERVO_CONFIG );
				WriteByte( (byte) ( pin & 127 )         );
				WriteIntAsTwoBytes( minPulse            );
				WriteIntAsTwoBytes( maxPulse            );
				WriteIntAsTwoBytes( angle               );
				WriteByte( (byte) Commands.SYSEX_END    );
				//Flush();
			}
			
			
			public void WriteDigitalPins( uint channel , uint pins )
			{
				WriteByte( (byte) Commands.DIGITAL_MESSAGE + channel );
				WriteIntAsTwoBytes( pins );
				//Flush();
			}
			
			
			public void WriteAnalogPin( uint pin , uint value )
			{
				Debug.Assert( pin   <  16 );
				Debug.Assert( value < 256 );

				WriteByte( (byte) Commands.ANALOG_MESSAGE + ( pin & 15 ) );
				WriteIntAsTwoBytes( (uint)value );
				//Flush();
			}
			
			
			public void ExtendedWriteAnalogPin( uint pin , uint value )
			{
				Debug.Assert( pin < 128 );
				
				WriteByte( (byte)Commands.SYSEX_START     );
				WriteByte( (byte)Commands.EXTENDED_ANALOG );
				WriteByte( (byte) ( pin   & 127 )         );
			    WriteByte( (byte) ( value & 127 )         );
				do {
					value = value >> 7;
					WriteByte( value & 127 );
				} while ( value > 0 ) ;
				WriteByte( (byte)Commands.SYSEX_END       );
				//Flush();
			}
			
			
			public void RequestReportVersion()
			{
				Debug.Log( "Serial  read timeout = " + _serial.ReadTimeout  );
				Debug.Log( "Serial write timeout = " + _serial.WriteTimeout );
				WriteByte( (byte) Commands.REPORT_VERSION );
				//Flush();
			}
			
			
			//FIRMATA2.0: SYSEX message to get version and name
			public void RequestFirmwareVersionAndName()
			{
				WriteSysexRequest( (byte) Commands.REPORT_VERSION );
			}
			
			
			//FIRMATA2.2: SYSEX message to get capabilities
			public void RequestCapabilities()
			{
				WriteSysexRequest( (byte) Commands.CAPABILITY_QUERY );
			}
			
			
			//FIRMATA2.2: SYSEX message to get current pin state
			public void RequestPinState( uint pin )
			{
				Debug.Assert( pin < 128 );
				
				WriteByte( (byte)Commands.SYSEX_START     );
				WriteByte( (byte)Commands.PIN_STATE_QUERY );
				WriteByte( (byte) ( pin & 127 )           );
				WriteByte( (byte)Commands.SYSEX_END       );
				
				//Flush();
			}
			
			
			public void SystemReset()
			{
				WriteByte( (byte)Commands.SYSTEM_RESET );
				//Flush();
			}
			
			
			public void SendI2CwriteRequest( uint slaveAddress , List<byte> data ) 
			{
				StartI2Crequest( slaveAddress , (byte)Commands.I2C_REQUEST_WRITE );
				for ( int i = 0 ; i < data.Count ; i++ ) {
					WriteIntAsTwoBytes( (uint)data[i] );
				}
				EndI2Crequest();
			}
			
			
			public void SendI2CreadOnceRequest( uint slaveAddress , uint numberOfBytes  )
			{
				StartI2Crequest( slaveAddress , (byte) Commands.I2C_REQUEST_READ_ONCE );
				WriteIntAsTwoBytes( numberOfBytes );
				EndI2Crequest();
			}
			
			
			public void SendI2CreadContiniouslyRequest( uint slaveAddress )
			{
				SendI2Crequest( slaveAddress , (byte) Commands.I2C_REQUEST_READ_CONTINUOUSLY );
			}
			
			
			public void SendI2CstopReadingRequest( uint slaveAddress )
			{
				SendI2Crequest( slaveAddress , (byte) Commands.I2C_REQUEST_STOP_READING );
			}
			
			
			private void SendI2Crequest( uint slaveAddress , byte addressMode )
			{
				StartI2Crequest( slaveAddress , addressMode );
				EndI2Crequest();
			}
			
			
			private void StartI2Crequest( uint slaveAddress , uint addressMode  )
			{
				if ( slaveAddress > 255 ) {
					addressMode |= (byte)Commands.I2C_10_BITS_ADDRESS_MODE;
					addressMode |= (byte)( ( slaveAddress >> 8 ) & 0x07 );
				}
				WriteByte( (byte) Commands.SYSEX_START    );
				WriteByte( (byte) Commands.I2C_REQUEST    );
				WriteByte( (byte) ( slaveAddress & 0xFF ) );
			    WriteByte( (byte) ( addressMode  & 0xFF ) );
			}
			
			
			private void EndI2Crequest()
			{
				WriteByte ( (byte)Commands.SYSEX_END );
				//Flush();
			}
			
			
			public void SendI2Crequests()
			{
				//Flush();
			}
			
			
			public void SendI2Cconfig( Boolean powerPinSetting , uint delay )
			{
				WriteByte( (byte)Commands.SYSEX_START                                     );
				WriteByte( (byte)Commands.I2C_CONFIG                                      );
				WriteByte( (byte)( powerPinSetting ? Commands.ENABLE : Commands.DISABLE ) );
				WriteIntAsTwoBytes( delay                                                 );
				WriteByte( (byte)Commands.SYSEX_END                                       );
			}
			
			
			public void SetSamplingInterval( uint intervalInMilliseconds )
			{
				WriteByte( (byte)Commands.SYSEX_START            );
				WriteByte( (byte)Commands.SAMPLING_INTERVAL      );
				WriteIntAsTwoBytes( intervalInMilliseconds );
				WriteByte( (byte)Commands.SYSEX_END              );
			}
			
			
			// ===================
			// Protected functions
			// ===================
			/**
			 * Write up to 14 bits of an integer as two separate 7bit-bytes
			 */
			protected void WriteIntAsTwoBytes( uint value )
			{
				Debug.Assert( value < 16384 );

				WriteByte(   value        & 127 );  // LSB (0-6) first
				WriteByte( ( value >> 7 ) & 127 );  // MSB (7-13) second
			}
			
			
			//region private functions

			// =================
			// Private functions
			// =================
			private void WriteSysexRequest( byte sysExCommand ) 
			{
				WriteByte( (byte)Commands.SYSEX_START );
				WriteByte(       sysExCommand         );
				WriteByte( (byte)Commands.SYSEX_END   );
				//Flush();
			}


			private void WriteByte( byte value )
			{
				_writeBuffer[ _nrOfBytes++ ] = value;

				Debug.Assert( _nrOfBytes < ( _writeBuffer.Length - 1 ) );
			}

			private void WriteByte( int value )
			{
				Debug.Assert( value >= 0 && value < 256 );
				
				WriteByte ( (byte) value );
			}
			
			private void WriteByte( uint value )
			{
				Debug.Assert( value < 256 );
				
				WriteByte ( (byte) value );
			}
			
			private void WriteBuffer()
			{
				//Debug.Log ( ">>> WriteBuffer()" );
				//OpenSerialPortIfNecessary();
				//Debug.Log ( "=== WriteBuffer()" );
				if ( _serial.IsOpen ) {
					_serial.Write( _writeBuffer, 0, _nrOfBytes );
					_nrOfBytes = 0;
				}
				//Debug.Log ( "<<< WriteBuffer()" );
			}

			#endregion
		}
	}
}


