using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1
{
    public class Client
    {
        public string Name { get; set; }
        public Client Opponent { get; set; }
        public bool IsPlaying { get; set; }
        public bool WaitingForMove { get; set; }
        public bool lookingForOpponent { get; set; }
        public string ConnectionId { get; set; }

        
    }

    public class Game:Hub
    {
        public static List<Client> _clients = new List<Client>();
        private object _syncRoot = new object();
        public void RegisterClient(string data)
        {
            lock (_syncRoot)
            {
                var client = _clients.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
                if (client == null)
                {
                    client = new Client { ConnectionId = Context.ConnectionId, Name = data };
                    _clients.Add(client);
                }
                  client.IsPlaying = false;
     
            }
            Clients.Client(Context.ConnectionId).registerComplete();
        }

        public void FindOpponent()
        {
            var player = _clients.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            if (player == null) return;

            player.lookingForOpponent = true;

            var opponent = _clients.Where(x => x.ConnectionId != Context.ConnectionId && x.lookingForOpponent && !x.IsPlaying).OrderBy(x => Guid.NewGuid()).FirstOrDefault();
            if (opponent == null)
            {
                Clients.Client(Context.ConnectionId).noOpponents();
                return;
            }

            player.IsPlaying = true;
            player.lookingForOpponent = false;
            opponent.IsPlaying = true;
            opponent.lookingForOpponent = false;

            player.Opponent = opponent;
            opponent.Opponent = player;

            Clients.Client(Context.ConnectionId).foundOpponent(opponent.Name);
            Clients.Client(opponent.ConnectionId).foundOpponent(player.Name);

        }

        public void LancarDados()
        {
            Random random = new Random();
            Clients.All.dado(random.Next(1, 7)); 
        }

       public void moverAll(double x1, double y1)
        {
            Clients.Others.move(x1,y1);
        }

        public void moverAll2(double x1, double y1)
        {
            Clients.Others.move2(x1, y1);
        }

        public void rpagina()
        {
            Clients.All.recaregar();
        }
    }
}