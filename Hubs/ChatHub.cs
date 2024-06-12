using ChatService.models;
using Microsoft.AspNetCore.SignalR;

namespace ChatService.Hubs
{
    public class ChatHub:Hub
    {
        private readonly string _botUser;
        private readonly IDictionary<string, UserConnection> _connection;

        public ChatHub(IDictionary<string, UserConnection> connection)
        {
            _connection = connection;
        }

     
        public async Task joinRoom(UserConnection userConnection)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userConnection.Room);
            _connection[Context.UserIdentifier] = userConnection;
            await Clients.Group(userConnection.Room).SendAsync("ReceiveMessage",_botUser , $"{userConnection.User} has joined {userConnection.Room}");
            await sendConnectedUser(userConnection.Room);
        }
        public async Task SendMessage(string message)
        {
            if(_connection.TryGetValue(Context.ConnectionId, out UserConnection userConnection))
            {
                await Clients.Group(userConnection.Room!)
                        .SendAsync("ReceiveMessage", userConnection.User,DateTime.Now);
            }
        }


        public Task OnDisConnectedAsync(Exception? exp)
        {
            if(!_connection.TryGetValue(Context.ConnectionId,out UserConnection userConnection))
            {
                return base.OnDisconnectedAsync(exp);
            }
            Clients.Group(userConnection.Room)
                    .SendAsync("ReceiveMessage", "Lets Program bot",$"{userConnection.User} has left the Group");
            sendConnectedUser(userConnection.Room!);
            {
                return base.OnDisconnectedAsync(exp);
            }
            
        }





        public Task sendConnectedUser(string room)
        {
            var users = _connection.Values.Where(u => u.Room == room)
                                    .Select(s => s.User);
            return Clients.Group(room).SendAsync("ConnectedUser", users);
        }
    }
}
