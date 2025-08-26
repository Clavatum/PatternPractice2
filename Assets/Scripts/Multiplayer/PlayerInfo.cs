using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;

public class PlayerInfo : NetworkBehaviour
{
    public static Dictionary<ulong, string> Nicknames = new Dictionary<ulong, string>();

    private NetworkVariable<FixedString32Bytes> playerName = new NetworkVariable<FixedString32Bytes>(
        writePerm: NetworkVariableWritePermission.Owner
    );

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            playerName.Value = MultiplayerMenuManager.PlayerNickname;
        }

        playerName.OnValueChanged += (oldValue, newValue) =>
        {
            Nicknames[OwnerClientId] = newValue.ToString();
        };

        Nicknames[OwnerClientId] = playerName.Value.ToString();
    }

    public override void OnNetworkDespawn()
    {
        if (Nicknames.ContainsKey(OwnerClientId))
            Nicknames.Remove(OwnerClientId);
    }

    public static string GetNickname(ulong clientId)
    {
        return Nicknames.TryGetValue(clientId, out var name) ? name : $"Client {clientId}";
    }
}