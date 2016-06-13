// ------------------------------------------------------------------------------
// Tracer.cs
//
// Released under MIT license: http://www.opensource.org/licenses/mit-license.php
// Copyright (C) 2015   Douwe A. van Twillert - Art & Technology, Saxion
// ------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Utilities
{
	public static class Tracer
	{
		public static String Bytes2Array( byte[] array , int count ) {
			String output = "";
			if ( count == -1 ) {
				count = array.Length;
			}
			for ( uint i = 0 ; i < count ; i++ ) {
				byte item = array[i];
				if ( output.Length > 0 ) {
					output = output + " , " + item.ToString( "X2" );
				} else {
					output = item.ToString( "X2" );
				}
			}
			return output;
		}

		public static String Bytes2Array( List<byte> array ) {
			String output = "";

			for ( int i = 0 ; i < array.Count ; i++ ) {
				byte item = array[i];
				if ( output.Length > 0 ) {
					output = output + " , " + item.ToString( "X2" );
				} else {
					output = item.ToString( "X2" );
				}
			}
			return output;
		}

		public static String traceArray<T>( List<T> array ) {
			String output = "";
			for ( int i = 0 ; i < array.Count ; i++ ) {
				if ( output.Length > 0 ) {
					output = output + " , " + array[i].ToString();
				} else {
					output = array[i].ToString();
				}
			}
			return output;
		}

/*
		public static String traceIntArray( List<int> array ) {
			String output = "";
			for ( uint i = 0 ; i < array.Count ; i++ ) {
				int item = array[(int)i];
				if ( output.Length > 0 ) {
					output = output + " , " + item;
				} else {
					output = item;
				}
			}
			return output;
		}
		*/

	}
}

