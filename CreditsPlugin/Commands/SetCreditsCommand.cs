using SharedLibraryCore;
using SharedLibraryCore.Commands;
using SharedLibraryCore.Configuration;
using SharedLibraryCore.Interfaces;
using EFClient = Data.Models.Client.EFClient;

namespace CreditsPlugin.Commands;

public class SetCreditsCommand : Command
{
    public SetCreditsCommand(CommandConfiguration config, ITranslationLookup translationLookup) :
        base(config, translationLookup)
    {
        Name = "setcredits";
        Description = "Set Credits";
        Alias = "scr";
        Permission = EFClient.Permission.Owner;
        RequiresTarget = false;
        Arguments = new[]
        {
            new CommandArgument
            {
                Name = "Player",
                Required = true
            },
            new CommandArgument
            {
                Name = "Amount",
                Required = true
            }
        };
    }

    public override Task ExecuteAsync(GameEvent gameEvent)
    {
        if (gameEvent.Type != GameEvent.EventType.Command) return Task.CompletedTask;

        var argStr = gameEvent.Data.Split(" ");

        if (!int.TryParse(argStr[1], out var argAmount))
        {
            gameEvent.Origin.Tell("(Color::Yellow)Error trying to parse second argument");
            return Task.CompletedTask;
        }

        gameEvent.Target = gameEvent.Owner.GetClientByName(argStr[0]).FirstOrDefault();

        if (gameEvent.Target == null)
        {
            gameEvent.Origin.Tell("(Color::Yellow)Error trying to find user");
            return Task.CompletedTask;
        }

        // Check if target isn't null - Set credits, sort, and tell the origin and target.
        if (gameEvent.Target != null)
        {
            gameEvent.Target.SetAdditionalProperty(Plugin.CreditsKey, Math.Abs(argAmount));
            gameEvent.Origin.Tell(
                $"Set credits for {gameEvent.Target.Name} (Color::White)to (Color::Cyan){Math.Abs(argAmount):N0}(Color::White)");
            if (gameEvent.Origin.ClientId != gameEvent.Target.ClientId)
                gameEvent.Target.Tell(
                    $"{gameEvent.Origin.Name} (Color::White)set your credits to (Color::Cyan){Math.Abs(argAmount):N0}(Color::White)");
            Plugin.PrimaryLogic?.OrderTop(gameEvent.Target, Math.Abs(argAmount));
        }

        return Task.CompletedTask;
    }
}
