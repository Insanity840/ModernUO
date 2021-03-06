using System;
using Server.Factions;
using Server.Mobiles;
using Server.Network;

namespace Server.Engines.ConPVP
{
  public class TournamentRegistrar : Banker
  {
    [Constructible]
    public TournamentRegistrar()
    {
      Timer.DelayCall(TimeSpan.FromSeconds(30.0), TimeSpan.FromSeconds(30.0), Announce_Callback);
    }

    public TournamentRegistrar(Serial serial) : base(serial)
    {
    }

    [CommandProperty(AccessLevel.GameMaster)]
    public TournamentController Tournament { get; set; }

    private void Announce_Callback()
    {
      Tournament tourney = Tournament?.Tournament;

      if (tourney?.Stage == TournamentStage.Signup)
        PublicOverheadMessage(MessageType.Regular, 0x35, false,
          "Come one, come all! Do you aspire to be a fighter of great renown? Join this tournament and show the world your abilities.");
    }

    public override void OnMovement(Mobile m, Point3D oldLocation)
    {
      base.OnMovement(m, oldLocation);

      Tournament tourney = Tournament?.Tournament;

      if (InRange(m, 4) && !InRange(oldLocation, 4) && tourney != null && tourney.Stage == TournamentStage.Signup &&
          m.CanBeginAction(this))
      {
        Ladder ladder = Ladder.Instance;

        LadderEntry entry = ladder?.Find(m);

        if (entry != null && Ladder.GetLevel(entry.Experience) < tourney.LevelRequirement)
          return;

        if (tourney.IsFactionRestricted && Faction.Find(m) == null) return;

        if (tourney.HasParticipant(m))
          return;

        PrivateOverheadMessage(MessageType.Regular, 0x35, false,
          $"Hello m'{(m.Female ? "Lady" : "Lord")}. Dost thou wish to enter this tournament? You need only to write your name in this book.",
          m.NetState);
        m.BeginAction(this);
        Timer.DelayCall(TimeSpan.FromSeconds(10.0), ReleaseLock_Callback, m);
      }
    }

    public void ReleaseLock_Callback(Mobile m)
    {
      m.EndAction(this);
    }

    public override void Serialize(IGenericWriter writer)
    {
      base.Serialize(writer);

      writer.Write(0);

      writer.Write(Tournament);
    }

    public override void Deserialize(IGenericReader reader)
    {
      base.Deserialize(reader);

      int version = reader.ReadInt();

      switch (version)
      {
        case 0:
          {
            Tournament = reader.ReadItem() as TournamentController;
            break;
          }
      }

      Timer.DelayCall(TimeSpan.FromSeconds(30.0), TimeSpan.FromSeconds(30.0), Announce_Callback);
    }
  }
}
