using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Hdg
{
	internal static class SerialisationHelpers
	{
		public enum ArrayElementType
		{
			Primitive,
			UserStruct,
			SerialiserInterface
		}

		private enum PrimitiveType
		{
			Byte,
			SByte,
			Int,
			UInt,
			Short,
			UShort,
			Long,
			ULong,
			Float,
			Double,
			Char,
			Bool,
			String,
			Decimal,
			Null
		}

		private static Type[] PrimitiveTypes = new Type[15]
		{
			typeof(byte),
			typeof(sbyte),
			typeof(int),
			typeof(uint),
			typeof(short),
			typeof(ushort),
			typeof(long),
			typeof(ulong),
			typeof(float),
			typeof(double),
			typeof(char),
			typeof(bool),
			typeof(string),
			typeof(decimal),
			null
		};

		public static void WriteList(BinaryWriter bw, IList list, ArrayElementType type)
		{
			bw.Write((int)type);
			if (list == null || list.Count == 0)
			{
				bw.Write(0);
			}
			else
			{
				int num = list.Count;
				bw.Write(num);
				switch (type)
				{
				case ArrayElementType.Primitive:
					WritePrimitiveList(bw, list);
					break;
				case ArrayElementType.SerialiserInterface:
					WriteSerialiserList(bw, list);
					break;
				case ArrayElementType.UserStruct:
					WriteUserStructList(bw, list);
					break;
				}
			}
		}

		public static void WritePrimitiveList(BinaryWriter bw, IList array)
		{
			int num = array.Count;
			Type elementType = array.GetType().GetListElementType();
			if (elementType == typeof(byte))
			{
				bw.Write(0);
				for (int i9 = 0; i9 < num; i9++)
				{
					bw.Write((byte)array[i9]);
				}
			}
			else if (elementType == typeof(sbyte))
			{
				bw.Write(1);
				for (int i8 = 0; i8 < num; i8++)
				{
					bw.Write((sbyte)array[i8]);
				}
			}
			else if (elementType == typeof(int))
			{
				bw.Write(2);
				for (int i7 = 0; i7 < num; i7++)
				{
					bw.Write((int)array[i7]);
				}
			}
			else if (elementType == typeof(uint))
			{
				bw.Write(3);
				for (int i6 = 0; i6 < num; i6++)
				{
					bw.Write((uint)array[i6]);
				}
			}
			else if (elementType == typeof(short))
			{
				bw.Write(4);
				for (int i5 = 0; i5 < num; i5++)
				{
					bw.Write((short)array[i5]);
				}
			}
			else if (elementType == typeof(ushort))
			{
				bw.Write(5);
				for (int i4 = 0; i4 < num; i4++)
				{
					bw.Write((ushort)array[i4]);
				}
			}
			else if (elementType == typeof(long))
			{
				bw.Write(6);
				for (int i3 = 0; i3 < num; i3++)
				{
					bw.Write((long)array[i3]);
				}
			}
			else if (elementType == typeof(ulong))
			{
				bw.Write(7);
				for (int i2 = 0; i2 < num; i2++)
				{
					bw.Write((ulong)array[i2]);
				}
			}
			else if (elementType == typeof(float))
			{
				bw.Write(8);
				for (int n = 0; n < num; n++)
				{
					bw.Write((float)array[n]);
				}
			}
			else if (elementType == typeof(double))
			{
				bw.Write(9);
				for (int m = 0; m < num; m++)
				{
					bw.Write((double)array[m]);
				}
			}
			else if (elementType == typeof(char))
			{
				bw.Write(10);
				for (int l = 0; l < num; l++)
				{
					bw.Write((char)array[l]);
				}
			}
			else if (elementType == typeof(bool))
			{
				bw.Write(11);
				for (int k = 0; k < num; k++)
				{
					bw.Write((bool)array[k]);
				}
			}
			else if (elementType == typeof(string))
			{
				bw.Write(12);
				for (int j = 0; j < num; j++)
				{
					bw.Write((string)array[j]);
				}
			}
			else if (elementType == typeof(decimal))
			{
				bw.Write(13);
				for (int i = 0; i < num; i++)
				{
					bw.Write((decimal)array[i]);
				}
			}
			else
			{
				RemoteDebugServer.Instance.SerializerRegistry.AddUnknownPrimitive(elementType);
				bw.Write(14);
			}
		}

		public static void WriteSerialiserList(BinaryWriter bw, IList array)
		{
			string fullName = array[0].GetType().FullName;
			bw.Write(fullName);
			for (int i = 0; i < array.Count; i++)
			{
				(array[i] as rdtSerializerInterface).Write(bw);
			}
		}

		public static void WriteUserStructList(BinaryWriter bw, IList array)
		{
			for (int i = 0; i < array.Count; i++)
			{
				List<rdtTcpMessageComponents.Property> subProperties = array[i] as List<rdtTcpMessageComponents.Property>;
				rdtTcpMessageComponents.Component.WriteProperties(bw, subProperties);
			}
		}

		public static void WritePrimitive(BinaryWriter bw, object value)
		{
			if (value == null)
			{
				bw.Write(14);
			}
			else
			{
				Type type = value.GetType();
				if (type == typeof(byte))
				{
					bw.Write(0);
					bw.Write((byte)value);
				}
				else if (type == typeof(sbyte))
				{
					bw.Write(1);
					bw.Write((sbyte)value);
				}
				else if (type == typeof(int))
				{
					bw.Write(2);
					bw.Write((int)value);
				}
				else if (type == typeof(uint))
				{
					bw.Write(3);
					bw.Write((uint)value);
				}
				else if (type == typeof(short))
				{
					bw.Write(4);
					bw.Write((short)value);
				}
				else if (type == typeof(ushort))
				{
					bw.Write(5);
					bw.Write((ushort)value);
				}
				else if (type == typeof(long))
				{
					bw.Write(6);
					bw.Write((long)value);
				}
				else if (type == typeof(ulong))
				{
					bw.Write(7);
					bw.Write((ulong)value);
				}
				else if (type == typeof(float))
				{
					bw.Write(8);
					bw.Write((float)value);
				}
				else if (type == typeof(double))
				{
					bw.Write(9);
					bw.Write((double)value);
				}
				else if (type == typeof(char))
				{
					bw.Write(10);
					bw.Write((char)value);
				}
				else if (type == typeof(bool))
				{
					bw.Write(11);
					bw.Write((bool)value);
				}
				else if (type == typeof(string))
				{
					bw.Write(12);
					bw.Write((string)value);
				}
				else if (type == typeof(decimal))
				{
					bw.Write(13);
					bw.Write((decimal)value);
				}
				else
				{
					RemoteDebugServer.Instance.SerializerRegistry.AddUnknownPrimitive(type);
					bw.Write(14);
				}
			}
		}

		public static void ReadList(BinaryReader br, out IList list, out ArrayElementType type)
		{
			type = (ArrayElementType)br.ReadInt32();
			int count = br.ReadInt32();
			list = null;
			if (count != 0)
			{
				switch (type)
				{
				case ArrayElementType.Primitive:
					list = ReadPrimitiveArray(br, count);
					break;
				case ArrayElementType.SerialiserInterface:
					list = ReadSerialiserArray(br, count);
					break;
				case ArrayElementType.UserStruct:
					list = ReadUserStructArray(br, count);
					break;
				}
			}
		}

		public static Array ReadPrimitiveArray(BinaryReader r, int count)
		{
			PrimitiveType primitiveType = (PrimitiveType)r.ReadInt32();
			Array array = Array.CreateInstance(typeof(object), count);
			ReadPrimitives(r, array, count, primitiveType);
			return array;
		}

		public static IList ReadPrimitiveList(BinaryReader r)
		{
			int num = r.ReadInt32();
			if (num == 0)
			{
				return null;
			}
			PrimitiveType primitiveType = (PrimitiveType)r.ReadInt32();
			Type type = PrimitiveTypes[(int)primitiveType];
			IList list = (IList)typeof(List<>).MakeGenericType(type).GetConstructor(Type.EmptyTypes).Invoke(null);
			ReadPrimitives(r, list, num, primitiveType);
			return list;
		}

		public static IList ReadSerialiserArray(BinaryReader br, int count)
		{
			Type type = Type.GetType(br.ReadString());
			IList array = new object[count];
			for (int i = 0; i < count; i++)
			{
				rdtSerializerInterface s = Activator.CreateInstance(type) as rdtSerializerInterface;
				s.Read(br);
				array[i] = s;
			}
			return array;
		}

		public static IList ReadUserStructArray(BinaryReader br, int count)
		{
			List<rdtTcpMessageComponents.Property>[] array = new List<rdtTcpMessageComponents.Property>[count];
			for (int i = 0; i < count; i++)
			{
				array[i] = rdtTcpMessageComponents.Component.ReadProperties(br);
			}
			return array;
		}

		private static void ReadPrimitives(BinaryReader r, IList array, int count, PrimitiveType primitiveType)
		{
			switch (primitiveType)
			{
			case PrimitiveType.Byte:
				for (int i9 = 0; i9 < count; i9++)
				{
					byte val14 = r.ReadByte();
					if (array.IsFixedSize)
					{
						array[i9] = val14;
					}
					else
					{
						array.Add(val14);
					}
				}
				break;
			case PrimitiveType.SByte:
				for (int i8 = 0; i8 < count; i8++)
				{
					sbyte val13 = r.ReadSByte();
					if (array.IsFixedSize)
					{
						array[i8] = val13;
					}
					else
					{
						array.Add(val13);
					}
				}
				break;
			case PrimitiveType.Int:
				for (int i7 = 0; i7 < count; i7++)
				{
					int val12 = r.ReadInt32();
					if (array.IsFixedSize)
					{
						array[i7] = val12;
					}
					else
					{
						array.Add(val12);
					}
				}
				break;
			case PrimitiveType.UInt:
				for (int i6 = 0; i6 < count; i6++)
				{
					uint val11 = r.ReadUInt32();
					if (array.IsFixedSize)
					{
						array[i6] = val11;
					}
					else
					{
						array.Add(val11);
					}
				}
				break;
			case PrimitiveType.Short:
				for (int i5 = 0; i5 < count; i5++)
				{
					short val10 = r.ReadInt16();
					if (array.IsFixedSize)
					{
						array[i5] = val10;
					}
					else
					{
						array.Add(val10);
					}
				}
				break;
			case PrimitiveType.UShort:
				for (int i4 = 0; i4 < count; i4++)
				{
					ushort val9 = r.ReadUInt16();
					if (array.IsFixedSize)
					{
						array[i4] = val9;
					}
					else
					{
						array.Add(val9);
					}
				}
				break;
			case PrimitiveType.Long:
				for (int i3 = 0; i3 < count; i3++)
				{
					long val8 = r.ReadInt64();
					if (array.IsFixedSize)
					{
						array[i3] = val8;
					}
					else
					{
						array.Add(val8);
					}
				}
				break;
			case PrimitiveType.ULong:
				for (int i2 = 0; i2 < count; i2++)
				{
					ulong val7 = r.ReadUInt64();
					if (array.IsFixedSize)
					{
						array[i2] = val7;
					}
					else
					{
						array.Add(val7);
					}
				}
				break;
			case PrimitiveType.Float:
				for (int n = 0; n < count; n++)
				{
					float val6 = r.ReadSingle();
					if (array.IsFixedSize)
					{
						array[n] = val6;
					}
					else
					{
						array.Add(val6);
					}
				}
				break;
			case PrimitiveType.Double:
				for (int m = 0; m < count; m++)
				{
					double val5 = r.ReadDouble();
					if (array.IsFixedSize)
					{
						array[m] = val5;
					}
					else
					{
						array.Add(val5);
					}
				}
				break;
			case PrimitiveType.Char:
				for (int l = 0; l < count; l++)
				{
					char val4 = r.ReadChar();
					if (array.IsFixedSize)
					{
						array[l] = val4;
					}
					else
					{
						array.Add(val4);
					}
				}
				break;
			case PrimitiveType.Bool:
				for (int k = 0; k < count; k++)
				{
					bool val3 = r.ReadBoolean();
					if (array.IsFixedSize)
					{
						array[k] = val3;
					}
					else
					{
						array.Add(val3);
					}
				}
				break;
			case PrimitiveType.String:
				for (int j = 0; j < count; j++)
				{
					string val2 = r.ReadString();
					if (array.IsFixedSize)
					{
						array[j] = val2;
					}
					else
					{
						array.Add(val2);
					}
				}
				break;
			case PrimitiveType.Decimal:
				for (int i = 0; i < count; i++)
				{
					decimal val = r.ReadDecimal();
					if (array.IsFixedSize)
					{
						array[i] = val;
					}
					else
					{
						array.Add(val);
					}
				}
				break;
			default:
				break;
			}
		}

		public static object ReadPrimitive(BinaryReader r)
		{
			switch (r.ReadInt32())
			{
			case 14:
				return null;
			case 0:
				return r.ReadByte();
			case 1:
				return r.ReadSByte();
			case 2:
				return r.ReadInt32();
			case 3:
				return r.ReadUInt32();
			case 4:
				return r.ReadInt16();
			case 5:
				return r.ReadUInt16();
			case 6:
				return r.ReadInt64();
			case 7:
				return r.ReadUInt64();
			case 8:
				return r.ReadSingle();
			case 9:
				return r.ReadDouble();
			case 10:
				return r.ReadChar();
			case 11:
				return r.ReadBoolean();
			case 12:
				return r.ReadString();
			case 13:
				return r.ReadDecimal();
			default:
				return null;
			}
		}
	}
}
