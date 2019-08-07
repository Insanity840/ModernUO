using System;
using System.IO;
using Server.Items;

namespace Server.Network
{
  public enum LRReason : byte
  {
    CannotLift,
    OutOfRange,
    OutOfSight,
    TryToSteal,
    AreHolding,
    Inspecific
  }

  public static partial class Packets
  {
    public static WriteDynamicPacketMethod<Item> WorldItem(out int length, Item item)
    {
      length = 20;

      static int write(Memory<byte> mem, int length, Item item)
      {
        SpanWriter w = new SpanWriter(mem.Span, 20);
        w.Write((byte)0x1A); // Packet ID
        w.Position += 2; // Dynamic Length

        uint serial = item.Serial.Value;
        int itemID = item.ItemID & 0x3FFF;
        int amount = item.Amount;
        Point3D loc = item.Location;
        int x = loc.m_X;
        int y = loc.m_Y;
        int hue = item.Hue;
        int flags = item.GetPacketFlags();
        int direction = (int)item.Direction;

        if (amount != 0)
          serial |= 0x80000000;
        else
          serial &= 0x7FFFFFFF;

        w.Write(serial);

        if (item is BaseMulti)
          w.Write((short)(itemID | 0x4000));
        else
          w.Write((short)itemID);

        if (amount != 0)
          w.Write((short)amount);

        x &= 0x7FFF;

        if (direction != 0) x |= 0x8000;

        w.Write((short)x);

        y &= 0x3FFF;

        if (hue != 0) y |= 0x8000;

        if (flags != 0) y |= 0x4000;

        w.Write((short)y);

        if (direction != 0)
          w.Write((byte)direction);

        w.Write((sbyte)loc.m_Z);

        if (hue != 0)
          w.Write((ushort)hue);

        if (flags != 0)
          w.Write((byte)flags);

        int bytesWritten = w.Position;
        w.Seek(1, SeekOrigin.Begin);
        w.Write((ushort)bytesWritten);

        return bytesWritten;
      }

      return write;
    }

    public static WriteFixedPacketMethod<Item> WorldItemSA(out int length)
    {
      length = 24;

      static void write(Memory<byte> mem, Item item)
      {
        SpanWriter w = new SpanWriter(mem.Span, 24);
        w.Write((byte)0xF3); // Packet ID

        w.Write((short)0x01);

        int itemID = item.ItemID;

        if (item is BaseMulti)
        {
          w.Write((byte)0x02);

          w.Write(item.Serial);

          itemID &= 0x3FFF;

          w.Write((short)itemID);

          w.Position++; // w.Write((byte)0);
        }
        else
        {
          w.Position++; // w.Write((byte)0);

          w.Write(item.Serial);

          itemID &= 0x7FFF;

          w.Write((short)itemID);

          w.Write((byte)item.Direction);
        }

        short amount = (short)item.Amount;
        w.Write(amount);
        w.Write(amount);

        Point3D loc = item.Location;
        w.Write((short)(loc.m_X & 0x7FFF));
        w.Write((short)(loc.m_Y & 0x3FFF));
        w.Write((sbyte)loc.m_Z);

        w.Write((byte)item.Light);
        w.Write((short)item.Hue);
        w.Write((byte)item.GetPacketFlags());
      }

      return write;
    }

    public static WriteFixedPacketMethod<Item> WorldItemHS(out int length)
    {
      length = 26;

      static void write(Memory<byte> mem, Item item)
      {
        SpanWriter w = new SpanWriter(mem.Span, 26);
        w.Write((byte)0xF3); // Packet ID

        w.Write((short)0x01);

        int itemID = item.ItemID;

        if (item is BaseMulti)
        {
          w.Write((byte)0x02);

          w.Write(item.Serial);

          itemID &= 0x3FFF;

          w.Write((short)itemID);

          w.Position++; // w.Write((byte)0);
        }
        else
        {
          w.Position++; // w.Write((byte)0);

          w.Write(item.Serial);

          itemID &= 0x7FFF;

          w.Write((short)itemID);

          w.Write((byte)item.Direction);
        }

        short amount = (short)item.Amount;
        w.Write(amount);
        w.Write(amount);

        Point3D loc = item.Location;
        w.Write((short)(loc.m_X & 0x7FFF));
        w.Write((short)(loc.m_Y & 0x3FFF));
        w.Write((sbyte)loc.m_Z);

        w.Write((byte)item.Light);
        w.Write((short)item.Hue);
        w.Write((byte)item.GetPacketFlags());

        // w.Position += 2; // w.Write((short)0); // ??
      }

      return write;
    }

    public static WriteFixedPacketMethod<LRReason> LiftRej(out int length)
    {
      length = 2;
      static void write(Memory<byte> mem, LRReason reason)
      {
        mem.Span[0] = 0x27; // Packet ID
        mem.Span[1] = (byte)reason;
      }

      return write;
    }
  }
}
