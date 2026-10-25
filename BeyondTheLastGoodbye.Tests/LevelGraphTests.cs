using System.Text.Json;
namespace BeyondTheLastGoodbye.Tests;

[TestFixture]
public class LevelGraphTests
{
    private string startRoomId = "";
    private Dictionary<string, List<string>> nextRooms = new Dictionary<string, List<string>>();

    private string FindRepoRoot()
    {
    DirectoryInfo? folder = new DirectoryInfo(AppContext.BaseDirectory);

    while (folder != null)
    {
        string filePath = Path.Combine(folder.FullName, "data", "levels.json");
        if (File.Exists(filePath))
        {
          return folder.FullName;
        }
        folder = folder.Parent;
    }

        throw new FileNotFoundException("could not find data/levels.json");
    }
    [SetUp]
    public void LoadLevel()
    {
    string path = Path.Combine(FindRepoRoot(), "data", "levels.json");
    string text = File.ReadAllText(path);
    JsonDocument document = JsonDocument.Parse(text);

    JsonElement level = document.RootElement.GetProperty("levels")[0];
    startRoomId = level.GetProperty("start_room_id").GetString() ?? "";

    nextRooms = new Dictionary<string, List<string>>();
    foreach (JsonElement room in level.GetProperty("rooms").EnumerateArray())
        {
        string roomId = room.GetProperty("id").GetString() ?? "";
        List<string> nextIds = new List<string>();

        foreach (JsonElement nextId in room.GetProperty("next_room_ids").EnumerateArray())
        {
            nextIds.Add(nextId.GetString() ?? "");
        }
            nextRooms[roomId] = nextIds;
        }
    }

    [Test]
    public void StartRoomExists()
    {
    bool exists = nextRooms.ContainsKey(startRoomId);
    Assert.That(exists, Is.True, "start_room_id '" + startRoomId + "' is not a room in the level");}
    [Test]
    public void EveryNextRoomIdPointsAtARealRoom()
    {
        string brokenLinks = "";

        foreach (string roomId in nextRooms.Keys)
        {
            foreach (string nextId in nextRooms[roomId])
            {
                if (!nextRooms.ContainsKey(nextId))
                {
                    brokenLinks = brokenLinks + roomId + " -> " + nextId + "  ";
                }
            }
        }

        Assert.That(brokenLinks, Is.EqualTo(""), "broken links: " + brokenLinks);
    }

    [Test]
    public void AllEightRoomsAreReachableFromStart()
    {
    Assert.That(nextRooms.Count, Is.EqualTo(8), "the level should have 8 rooms");
    Assert.That(nextRooms.ContainsKey(startRoomId), Is.True, "start room does not exist");

    HashSet<string> visited = new HashSet<string>();
    Queue<string> toVisit = new Queue<string>();

    visited.Add(startRoomId);
    toVisit.Enqueue(startRoomId);

    while (toVisit.Count > 0)
    {
        string current = toVisit.Dequeue();

        foreach (string nextId in nextRooms[current])
        {
            if (nextRooms.ContainsKey(nextId) && !visited.Contains(nextId))
            {
                visited.Add(nextId);
                toVisit.Enqueue(nextId);
             }
        }
        }

        string strandedRooms = "";
        foreach (string roomId in nextRooms.Keys)
        {
            if (!visited.Contains(roomId))
            {
                strandedRooms = strandedRooms + roomId + "  ";
            }
        }

        Assert.That(strandedRooms, Is.EqualTo(""), "unreachable rooms: " + strandedRooms);
    }

    [Test]
    public void LastRoomHasNoNextRooms()
    {
        int endRoomCount = 0;
        string endRooms = "";

        foreach (string roomId in nextRooms.Keys)
        {
            if (nextRooms[roomId].Count == 0)
            {
                endRoomCount = endRoomCount + 1;
                endRooms = endRooms + roomId + "  ";
        }
        }

    Assert.That(endRoomCount, Is.EqualTo(1), "expected exactly 1 last room, found: " + endRooms); }
}