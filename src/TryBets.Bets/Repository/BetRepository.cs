using TryBets.Bets.DTO;
using TryBets.Bets.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace TryBets.Bets.Repository;

public class BetRepository : IBetRepository
{
    protected readonly ITryBetsContext _context;
    public BetRepository(ITryBetsContext context)
    {
        _context = context;
    }

    public BetDTOResponse Post(BetDTORequest betRequest, string email)
    {
        User usuarioEncontrado = _context.Users.FirstOrDefault(x => x.Email == email)!;
        if (usuarioEncontrado == null) throw new Exception("User not founded");

        Match partidaEncontrada = _context.Matches.FirstOrDefault(m => m.MatchId == betRequest.MatchId)!;
        if (partidaEncontrada == null) throw new Exception("Match not founded");

        Team timeEncontrado = _context.Teams.FirstOrDefault(t => t.TeamId == betRequest.TeamId)!;
        if (timeEncontrado == null) throw new Exception("Team not founded");

        if (partidaEncontrada.MatchFinished) throw new Exception("Match finished");

        if (partidaEncontrada.MatchTeamAId != betRequest.TeamId && partidaEncontrada.MatchTeamBId != betRequest.TeamId) throw new Exception("Team is not in this match");

        Bet newBet = new Bet
        {
            UserId = usuarioEncontrado.UserId,
            MatchId = betRequest.MatchId,
            TeamId = betRequest.TeamId,
            BetValue = betRequest.BetValue
        };

        _context.Bets.Add(newBet);
        _context.SaveChanges();

        Bet createdBet = _context.Bets.Include(b => b.Team).Include(b => b.Match).Where(b => b.BetId == newBet.BetId).FirstOrDefault()!;

        return new BetDTOResponse
        {
            BetId = createdBet.BetId,
            MatchId = createdBet.MatchId,
            TeamId = createdBet.TeamId,
            BetValue = createdBet.BetValue,
            MatchDate = createdBet.Match!.MatchDate,
            TeamName = createdBet.Team!.TeamName,
            Email = createdBet.User!.Email
        };
    }
    public BetDTOResponse Get(int BetId, string email)
    {
        User usuarioEncontrado = _context.Users.FirstOrDefault(x => x.Email == email)!;
        if (usuarioEncontrado == null) throw new Exception("User not founded");

        Bet apostaEncontrada = _context.Bets.Include(b => b.Team).Include(b => b.Match).Where(b => b.BetId == BetId).FirstOrDefault()!;
        if (apostaEncontrada == null) throw new Exception("Bet not founded");

        if (apostaEncontrada.User!.Email != email) throw new Exception("Bet view not allowed");

        return new BetDTOResponse
        {
            BetId = apostaEncontrada.BetId,
            MatchId = apostaEncontrada.MatchId,
            TeamId = apostaEncontrada.TeamId,
            BetValue = apostaEncontrada.BetValue,
            MatchDate = apostaEncontrada.Match!.MatchDate,
            TeamName = apostaEncontrada.Team!.TeamName,
            Email = apostaEncontrada.User!.Email
        };
    }
}