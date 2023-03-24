using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.IO;
using System.Text;

namespace UniquePairings
{
	class Program
	{
		// Add all the participants here!
		// note: works best with even number of participants
		static readonly List<string> Participants = new List<string>
		{
			"TJ",
			"Diana",
			"JD",
			"Pryor",
			"Advay",
			"Shane",
			"Rachel",
			"James",
			"Matthew",
			"Heng",

			"Kunyi",
			//"Alex",
			//"Rachel",
			//"Chris",
			
	
			//"Kunyi",
		};

		// If we have already met with some groups, put in the groups here
		// so we can create more pairings and not redo partners
		static readonly List<Group> AlreadyPairedGroups = new List<Group>
		{
			//new Group("TJ", "Diana", "JD"),
			//new Group("Pryor", "Advay", "Shane"),
			//new Group("Rachel", "James"),
			//new Group("Matthew", "DoRon"),
			//new Group("Matthew", "Advay"),
			//new Group("TJ", "Diana", "JD"),
			//new Group("TJ", "Rachel"),
			//new Group("Jim", "Pam"),
			//new Group("Matt1", "Kunyi1"),
			//new Group("Advay1", "Heng1"),
			//new Group("TJ1", "Rachel1"),
			//new Group("Jim1", "Pam1"),
			//new Group("Matt", "Kunyi"),
			//new Group("Advay", "Heng"),
			//new Group("TJ", "Jim"),
			//new Group("Rachel", "Pam"),
			//new Group("Matt1", "Advay1"),
			//new Group("Kunyi1", "Heng1"),
			//new Group("TJ1", "Jim1"),
			//new Group("Rachel1", "Pam1"),
			//new Group("Jim2", "Pam2"),
			//new Group("DoRon", "Alex"),
			//new Group("Manuel", "Rolf"),
			//new Group("Steve", "Dustin"),
		};

		static int DesiredRounds;
		static StringBuilder SB = new StringBuilder();
		static int NumberOf2 = 0;
		static int NumberOf3 = 0;
		static bool PrioritizeThrees = false;
		static int GroupTotal = 0;
		static bool Debugging = false;

		static void Main(string[] args)
		{
			FindPairingNumbers(Participants.Count);

			//var myTimer = new Timer(TimerCallback, null, 0, 4000);
			//DesiredRounds = Participants.Count > 6 ? 6 : Participants.Count / 2;
			DesiredRounds = 6;
			//DesiredRounds = Participants.Count / 2;


			Console.WriteLine($"Starting with DesiredRound: {DesiredRounds}");

			CreateUniquePairing();
			WriteResults();
		}

		static void FindPairingNumbers (int i)
		{
			var total2s = 0;
			var total3s = 0;
			var count = i;

			if (count == 0 || count == 1) {
				Console.WriteLine($"Starting with i: {i}. Cannot form groups");
				NumberOf2 = 0;
				NumberOf3 = 0;
				return;
			}

			while (count > 0) {
				if (PrioritizeThrees) {
					if (count % 3 == 0)
					{
						total3s += count / 3;
						count = 0;
					}

					else if (count % 3 == 1)
					{
						count -= 2;
						total2s++;
					}

					else if (count % 3 == 2)
					{
						total2s++;
						count -= 2;
						total3s = count / 3;
						count = 0;
					}
				}

				// otherwise we will prioritize groups of twos
				else {
					if (count % 2 == 1) {
						count -= 3;
						total3s++;
					}

					total2s = count / 2;
					count = 0;
				}
			}

			Console.WriteLine($"Starting with i: {i}. {total2s} twos and {total3s} threes");
			NumberOf2 = total2s;
			NumberOf3 = total3s;
			GroupTotal = total2s + total3s;

			if (Debugging) {
				SB.AppendLine($"Number of 2s: {NumberOf2} - Number of 3s: {NumberOf3} - GroupTotal: {GroupTotal}");
			}
			return;
		}

		static void CreateUniquePairing()
		{
			var comparer = new PairedComparer();
			var PossiblePairings = new HashSet<Group>(comparer);
			var AlreadyPaired = new HashSet<Group>(comparer);
			var remainingParticipants = new List<string>();
			var OriginalPairedGroups = new List<Group>();

			// Add in all the groups who have already met if any
			foreach (var pairing in AlreadyPairedGroups)
			{
				AlreadyPaired.Add(pairing);
				OriginalPairedGroups.Add(pairing);

				if (pairing.Count == 3) {
					AlreadyPaired.Add(new Group(pairing.Partner, pairing.Person));
					AlreadyPaired.Add(new Group(pairing.Third, pairing.Person));
					AlreadyPaired.Add(new Group(pairing.Partner, pairing.Third));
					OriginalPairedGroups.Add(new Group(pairing.Partner, pairing.Person));
					OriginalPairedGroups.Add(new Group(pairing.Third, pairing.Person));
					OriginalPairedGroups.Add(new Group(pairing.Partner, pairing.Third));
				}
			}

			// Get all possible combinations of participants
			foreach (var participant in Participants)
			{
				foreach (var partner in Participants.Where(p => p != participant))
				{
					foreach (var third in Participants.Where (p => p != participant && p != partner))
					{
						var newGroupThird = new Group(participant, partner, third);

						if (CheckForGroupMembers(newGroupThird, "DoRon", "Diana", "Advay")) {
						}

						// if we have manually inputted this group, do not pair them again
						if (!AlreadyPaired.Add(newGroupThird))
							continue;

						PossiblePairings.Add(new Group(participant, partner, third));
					}

					var newGroup = new Group(participant, partner);
					// if we have manually inputted this group, do not pair them again
					if (!AlreadyPaired.Add(newGroup))
						continue;

					PossiblePairings.Add(new Group(participant, partner));
				}
			}


			// Use Backtracking to give us the best combinations
			var groups = new List<Group>();
			groups.AddRange(PossiblePairings);

			var pairedGroups = new List<Group>();
			pairedGroups.AddRange(OriginalPairedGroups);

			var isPossible = BackTrackingPairings(groups, pairedGroups, 0, new List<Round>(), NumberOf2, NumberOf3);
			while (DesiredRounds > 0 && !isPossible)
			{
				SB.AppendLine($"Did not find a fully working solution with {DesiredRounds}\nLet's try with {DesiredRounds - 1}");
				DesiredRounds -= 1;
				pairedGroups = new List<Group>();
				pairedGroups.AddRange(OriginalPairedGroups);
				isPossible = BackTrackingPairings(groups, pairedGroups, 0, new List<Round>(), NumberOf2, NumberOf3);
			}
		}

		// We start this method with all the groups, but we can keep calling this
		// method recursively with less groups each time and see if we can produce
		// the correct number of pairings, if not, pop out a recursion and try the next
		// combination!
		static bool BackTrackingPairings(List<Group> groups,List<Group> pairedGroups, int roundNumber, List<Round> workingSet, int numberOf2s, int numberOf3s)
		{
			//// base case, we hit enough round numbers
			if (roundNumber >= DesiredRounds)
			{
				LogWorkingSet(workingSet);
				return true;
			}

			var remainingGroups = new List<Group>();
			remainingGroups.AddRange(groups);

			foreach (var group in groups.Where (g => g.Count == 3))
			{
				if (numberOf3s == 0)
					break;

				if (roundNumber == 1 && CheckForGroupMembers(group, "DoRon", "Diana", "Advay"))
				{
					if (workingSet.Count == 2 && workingSet[1].Groups.Count == 1 &&
						CheckForGroupMembers(workingSet[1].Groups[0], "TJ", "Pryor", "Rachel")) {
					}
				}

				if (IsValidPairing(group, pairedGroups, roundNumber, workingSet, out var nextRoundNumber))
				{
					if (!remainingGroups.Remove(group))
						Console.WriteLine("ERROR");

					// add this group to the workingSet's current round
					AddToWorkingSet(ref workingSet, group, roundNumber);
					pairedGroups.Add(new Group(group.Partner, group.Person));
					pairedGroups.Add(new Group(group.Partner, group.Third));
					pairedGroups.Add(new Group(group.Third, group.Person));
					numberOf3s--;

					if (Debugging)
						SB.AppendLine($"    + Adding {group.Person} + {group.Partner} + {group.Third}");

					if (Debugging && nextRoundNumber > roundNumber)
						SB.AppendLine($"Upping the Round to {nextRoundNumber}");

					var new2 = nextRoundNumber > roundNumber ? NumberOf2 : numberOf2s;
					var new3 = nextRoundNumber > roundNumber ? NumberOf3 : numberOf3s;

					var newPaired = new List<Group>();
					newPaired.AddRange(pairedGroups);

					if (BackTrackingPairings(remainingGroups, newPaired, nextRoundNumber, workingSet, new2, new3))
						return true;

					if (Debugging)
						SB.AppendLine($"    - Removing {group.Person} + {group.Partner} + {group.Third}");

					// if we get back here, we should remove the last Group and continue our search
					remainingGroups.Add(group);
					pairedGroups.Remove(new Group(group.Partner, group.Person));
					pairedGroups.Remove(new Group(group.Partner, group.Third));
					pairedGroups.Remove(new Group(group.Third, group.Person));
					RemoveFromWorkingSet(ref workingSet, group, roundNumber);
					numberOf3s++;
				}
			}

			foreach (var group in groups.Where(g => g.Count == 2))
			{
				if (numberOf2s == 0)
					break;

				if (group.Partner == "Diana" && group.Person == "TJ") {
				}

				if (IsValidPairing(group, pairedGroups, roundNumber, workingSet, out var nextRoundNumber))
				{
					if (!remainingGroups.Remove(group))
						Console.WriteLine("ERROR");

					// add this group to the workingSet's current round
					AddToWorkingSet(ref workingSet, group, roundNumber);
					pairedGroups.Add(new Group(group.Partner, group.Person));
					numberOf2s--;

					if (Debugging)
						SB.AppendLine($"    + Adding {group.Person} + {group.Partner}");

					if (Debugging && nextRoundNumber > roundNumber)
						SB.AppendLine($"Upping the Round to {nextRoundNumber}");

					var new2 = nextRoundNumber > roundNumber ? NumberOf2 : numberOf2s;
					var new3 = nextRoundNumber > roundNumber ? NumberOf3 : numberOf3s;

					var newPaired = new List<Group>();
					newPaired.AddRange(pairedGroups);

					if (BackTrackingPairings(remainingGroups, newPaired, nextRoundNumber, workingSet, new2, new3))
						return true;

					if (Debugging)
						SB.AppendLine($"    - Removing {group.Person} + {group.Partner}");

					// if we get back here, we should remove the last Group and continue our search
					remainingGroups.Add(group);
					RemoveFromWorkingSet(ref workingSet, group, roundNumber);
					pairedGroups.Remove(new Group(group.Partner, group.Person));
					numberOf2s++;
				}
			}

			return false;
		}

		static void TimerCallback(object? state)
		{
			// This method will be executed every 5 seconds
			if (DesiredRounds > 1)
				DesiredRounds--;
			Console.WriteLine($"Lowering DesiredRound: {DesiredRounds}");
		}

		static bool IsValidPairing(Group group, List<Group> pairedGroups, int roundNumber, List<Round> workingSet, out int nextRoundNumber)
		{
			var foundGroups = 0;
			nextRoundNumber = roundNumber;

			// find the corresponding round
			foreach (var round in workingSet)
			{
				if (round.RoundNumber == roundNumber)
				{
					foreach (var g in round.Groups) {
						if (group.Count == 3)
						{
							if (IsAlreadyParticipating(new Group(group.Partner, group.Person), g) ||
								IsAlreadyParticipating(new Group(group.Third, group.Person), g) ||
								IsAlreadyParticipating(new Group(group.Partner, group.Third), g))
							{
								return false;
							}
						}
						else
						{
							if (IsAlreadyParticipating(group, g))
								return false;
						}
					}
					foundGroups = round.Groups.Count;
				}
			}

			foreach (var pair in pairedGroups) {
				if (group.Count == 3) {
					if (GroupsAreSame(new Group(group.Partner, group.Person), pair) ||
						GroupsAreSame(new Group(group.Third, group.Person), pair) ||
						GroupsAreSame(new Group(group.Partner, group.Third), pair))
					{
						return false;
					}
				}
				else {
					if (GroupsAreSame(group, pair))
						return false;
				}
			}

			// if we will be at the max round group size after adding this, increase the groupsize
			if ((foundGroups + 1) == GroupTotal)
			{
				//if (workingSet.Where (s => s.RoundNumber == roundNumber).Select(s => s.Groups.Count).FirstOrDefault() - 1 == GroupTotal)
				nextRoundNumber++;
			}

			return true;
		}

		static bool RoundCanGoHere(List<Group> groups, Group newGroup)
		{
			foreach (var group in groups)
			{
				if (IsAlreadyParticipating(group, newGroup))
					return false;
				if (GroupsAreSame(group, newGroup))
					return false;
			}
			return true;
		}

		static bool IsAlreadyParticipating(Group x, Group y)
		{
			if (x.Count == 2 && y.Count == 2)
			{
				if (x.Partner == y.Partner || x.Person == y.Person)
					return true;
				if (x.Partner == y.Person || x.Person == y.Partner)
					return true;
			}

			else if (x.Count == 3 && y.Count == 3)
			{
				var isPersonMatch = (x.Person == y.Person ||
					x.Person == y.Partner ||
					x.Person == y.Third);

				var isPartnerMatch = (x.Partner == y.Person ||
					x.Partner == y.Partner ||
					x.Partner == y.Third);

				var isThirdMatch = (x.Third == y.Person ||
					x.Third == y.Partner ||
					x.Third == y.Third);

				if (isPersonMatch || isPartnerMatch || isThirdMatch)
					return true;
			}

			else if (x.Count == 2 && y.Count == 3)
			{
				var isPersonMatch = (x.Person == y.Person ||
					x.Person == y.Partner ||
					x.Person == y.Third);

				var isPartnerMatch = (x.Partner == y.Person ||
					x.Partner == y.Partner ||
					x.Partner == y.Third);

				if (isPersonMatch || isPartnerMatch)
					return true;
			}

			else if (x.Count == 3 && y.Count == 2)
			{
				var isPersonMatch = (x.Person == y.Person ||
					x.Person == y.Partner);

				var isPartnerMatch = (x.Partner == y.Person ||
					x.Partner == y.Partner);

				var isThirdMatch = (x.Third == y.Person ||
					x.Third == y.Partner);

				if (isPersonMatch || isPartnerMatch || isThirdMatch)
					return true;
			}

			return false;
		}

		static bool GroupsAreSame(Group x, Group y)
		{
			if (x is null && y is null)
				return true;
			if (x is null || y is null)
				return false;
			//if (x.Partner == y.Partner && x.Person == y.Person)
			//	return true;
			//if (x.Partner == y.Person && x.Person == y.Partner)
			//	return true;

			//if (x.Count != y.Count)
			//	return false;

			if (x.Count == 2 && y.Count == 2)
			{
				if (x.Partner == y.Partner && x.Person == y.Person)
					return true;
				if (x.Partner == y.Person && x.Person == y.Partner)
					return true;
			}

			else if (x.Count == 3 && y.Count == 3)
			{
				var isPersonMatch = (x.Person == y.Person ||
					x.Person == y.Partner ||
					x.Person == y.Third);

				var isPartnerMatch = (x.Partner == y.Person ||
					x.Partner == y.Partner ||
					x.Partner == y.Third);

				var isThirdMatch = (x.Third == y.Person ||
					x.Third == y.Partner ||
					x.Third == y.Third);

				if (isPersonMatch && isPartnerMatch && isThirdMatch)
					return true;
			}

			else if (x.Count == 2 && y.Count == 3)
			{
				var isPersonMatch = (x.Person == y.Person ||
					x.Person == y.Partner ||
					x.Person == y.Third);

				var isPartnerMatch = (x.Partner == y.Person ||
					x.Partner == y.Partner ||
					x.Partner == y.Third);

				if (isPersonMatch && isPartnerMatch)
					return true;
			}

			else if (x.Count == 3 && y.Count == 2)
			{
				var isPersonMatch = (x.Person == y.Person ||
					x.Person == y.Partner);

				var isPartnerMatch = (x.Partner == y.Person ||
					x.Partner == y.Partner);

				var isThirdMatch = (x.Third == y.Person ||
					x.Third == y.Partner);

				if (isPersonMatch && isPartnerMatch && isThirdMatch)
					return true;
			}

			return false;
		}

		static void AddToWorkingSet(ref List<Round> workingset, Group group, int roundNumber)
		{
			bool isFound = false;

			// look for the round and add the group
			foreach (var round in workingset)
			{
				if (round.RoundNumber == roundNumber)
				{
					isFound = true;
					round.Add(group);
					break;
				}
			}
			// if we don't already have this round, create it and add group
			if (!isFound)
				workingset.Add(new Round(roundNumber, new List<Group>() { group }));
		}

		static void RemoveFromWorkingSet(ref List<Round> workingset, Group group, int roundNumber)
		{
			// look for the round and remove the group
			foreach (var round in workingset)
			{
				if (round.RoundNumber == roundNumber)
				{
					round.Remove(group);
					return;
				}
			}
		}

		static void LogPairings(IEnumerable<Group> groups)
		{
			foreach (var group in groups)
			{
				if (group.Count == 2)
					SB.AppendLine($"new Group(\"{group.Person}\", \"{group.Partner}\"),");
				else if (group.Count == 3)
					SB.AppendLine($"new Group(\"{group.Person}\", \"{group.Partner}\", \"{group.Third}\"),");
			}
		}

		static void LogWorkingSet(List<Round> rounds)
		{
			var roundNumber = 1;
			foreach (var round in rounds)
			{
				SB.AppendLine($"\nRound: {round.RoundNumber + 1}");
				LogPairings(round.Groups);
				roundNumber++;
			}
		}

		static void WriteResults()
		{
			Console.WriteLine(SB.ToString());

			var outputFile = new StreamWriter("./../../../output.txt", false);
			outputFile.WriteLine(SB.ToString());
			outputFile.Close();
		}

		static bool CheckForGroupMembers(Group group, string person1, string person2, string person3)
		{
			var isPerson1 = (group.Person == person1 || group.Partner == person1 || group.Third == person1);
			var isPerson2 = (group.Person == person2 || group.Partner == person2 || group.Third == person2);
			var isPerson3 = (group.Person == person3 || group.Partner == person3 || group.Third == person3);

			return isPerson1 && isPerson2 && isPerson3;
		}
	}

	public class Group
	{
		public string Person { get; set; }
		public string Partner { get; set; }
		public string Third { get; set; } = string.Empty;
		public int Count;

		public Group(string person, string partner)
		{
			Person = person;
			Partner = partner;
			Count = 2;
		}

		public Group(string person, string partner, string third)
		{
			Person = person;
			Partner = partner;
			Third = third;
			Count = 3;
		}
	}

	public class Round
	{
		public List<Group> Groups { get; set; }
		public int RoundNumber { get; set; }

		public Round(int roundNumber, List<Group> groups)
		{
			RoundNumber = roundNumber;
			Groups = groups;
		}

		public void Add(Group group)
		{
			Groups.Add(group);
		}

		public bool Remove(Group group)
		{
			return Groups.Remove(group);
		}
	}

	public class PairedComparer : IEqualityComparer<Group>
	{
		public bool Equals(Group? x, Group? y)
		{
			if (x is null && y is null)
				return true;
			if (x is null || y is null)
				return false;

			if (x.Count == 2 && y.Count == 2)
			{
				if (x.Partner == y.Partner && x.Person == y.Person)
					return true;
				if (x.Partner == y.Person && x.Person == y.Partner)
					return true;
			}

			else if (x.Count == 3 && y.Count == 3)
			{
				var isPersonMatch = (x.Person == y.Person ||
					x.Person == y.Partner ||
					x.Person == y.Third);

				var isPartnerMatch = (x.Partner == y.Person ||
					x.Partner == y.Partner ||
					x.Partner == y.Third);

				var isThirdMatch = (x.Third == y.Person ||
					x.Third == y.Partner ||
					x.Third == y.Third);

				if (isPersonMatch && isPartnerMatch && isThirdMatch)
					return true;
			}

			else if (x.Count == 2 && y.Count == 3)
			{
				var isPersonMatch = (x.Person == y.Person ||
					x.Person == y.Partner ||
					x.Person == y.Third);

				var isPartnerMatch = (x.Partner == y.Person ||
					x.Partner == y.Partner ||
					x.Partner == y.Third);

				if (isPersonMatch && isPartnerMatch)
					return true;
			}

			else if (x.Count == 3 && y.Count == 2)
			{
				var isPersonMatch = (x.Person == y.Person ||
					x.Person == y.Partner);

				var isPartnerMatch = (x.Partner == y.Person ||
					x.Partner == y.Partner);

				var isThirdMatch = (x.Third == y.Person ||
					x.Third == y.Partner);

				if (isPersonMatch && isPartnerMatch && isThirdMatch)
					return true;
			}

			return false;
		}

		public int GetHashCode([DisallowNull] Group obj)
		{
			if (obj.Count == 2) {
				var one = string.Compare(obj.Person, obj.Partner);
				if (one == -1)
					return string.Concat(obj.Person, obj.Partner).GetHashCode();

				return string.Concat(obj.Partner, obj.Person).GetHashCode();
			}
			else {
				string[] sortedStrings = { obj.Person, obj.Partner, obj.Third };
				Array.Sort(sortedStrings);
				return (sortedStrings[0] + sortedStrings[1] + sortedStrings[2]).GetHashCode();
			}
			
		}
	}
}
