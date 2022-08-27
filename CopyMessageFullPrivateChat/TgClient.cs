using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TL;

namespace TrackingAndCopyingMessagesFromPrivateChannel
{
    internal class TgClient
    {
        private WTelegram.Client Client { get; set; }
        private User My { get; set; }
        private readonly Dictionary<long, User> Users = new();
        private readonly Dictionary<long, ChatBase> Chats = new();
        private List<long> ChatsForMessages = new List<long>();
        private long PrivateChat = 0;


        private string Config(string what)
        {
            switch (what)
            {
                case "api_id": Console.Write("API Id: "); return "9650623";
                case "api_hash": Console.Write("API Hash: "); return "ec5bb72cbd45cb422e20ced2346ffba6";
                case "phone_number": Console.Write("Phone number: "); return Console.ReadLine();
                case "verification_code": Console.Write("Verification code: "); return Console.ReadLine();  // if sign-up is required
                case "password": Console.Write("Password: "); return Console.ReadLine();     // if user has enabled 2FA
                default: return null;                  // let WTelegramClient decide the default config
            }
        }

        public async Task CopyMashine()
        {
            Console.Write("В сколько чатов/каналов вы хотите копировать сообщения из приватного канала?: ");
            var amountChats = Convert.ToInt32(Console.ReadLine());

            Client = new WTelegram.Client(Config);

            using (Client)
            {
                Client.OnUpdate += Client_OnUpdate;
                My = await Client.LoginUserIfNeeded();

                var chats = await Client.Messages_GetAllChats();
                foreach (var (id, chat) in chats.chats)
                {
                    switch (chat)
                    {
                        case Chat basicChat when basicChat.IsActive:
                            Console.WriteLine($"{id}:  Basic chat: {basicChat.title} with {basicChat.participants_count} members");
                            break;
                        case Channel group when group.IsGroup:
                            Console.WriteLine($"{id}: Group {group.username}: {group.title}");
                            break;
                        case Channel channel:
                            Console.WriteLine($"{id}: Channel {channel.username}: {channel.title}");
                            break;
                    }
                }

                Console.Write("Введите ID приватного чата, из которого необходимо копировать новые сообщения: ");
                PrivateChat = Convert.ToInt64(Console.ReadLine());

                Console.WriteLine("Введите поочередно ID чатов/каналов в которые вы хотите копировать сообщения (после каждого ID нажимайте клавишу Enter): ");
                for (int i = 0; i < amountChats; i++)
                {
                    ChatsForMessages.Add(Convert.ToInt64(Console.ReadLine()));
                }

                Users[My.id] = My;

                var dialogs = await Client.Messages_GetAllDialogs();
                dialogs.CollectUsersChats(Users, Chats);
                Console.ReadKey();
            }
        }

        private async Task Client_OnUpdate(IObject arg)
        {
            if (arg is not UpdatesBase updates)
            {
                return;
            }
            updates.CollectUsersChats(Users, Chats);
            foreach (var update in updates.UpdateList)
                switch (update)
                {
                    case UpdateNewMessage
                    unm:
                        await DisplayMessage(unm.message);
                        break;
                    default: Console.WriteLine(update.GetType().Name); break;
                }
        }

        private async Task DisplayMessage(MessageBase messageBase, bool edit = false)
        {
            if (edit) Console.Write("(Edit): ");
            switch (messageBase)
            {
                case Message m:
                    switch (m.peer_id.ID)
                    {
                        case var privateChat when privateChat == PrivateChat:
                            foreach (var chat in ChatsForMessages)
                            {
                                try
                                {
                                    await Client.SendMessageAsync(Chats[chat], m.message);
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e);
                                }
                            }
                            break;
                    }
                    break;
            }
        }
    }
}
