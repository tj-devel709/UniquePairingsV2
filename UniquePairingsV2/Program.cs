using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace UniquePairings
{
	// TODO Print results to a file so that I can copy and paste things easier!

	class Program
	{
		// Add all the participants here!
		// note: works best with even number of participants
		static readonly List<string> Participants = new List<string>
		{
			"Matt",
			"Kunyi",
			"Advay",
			"Heng",
			"TJ",
			"Rachel",
		};

		// If we have already met with some groups, put in the groups here
		// so we can create more pairings and not redo partners
		static readonly List<Group> AlreadyPairedGroups = new List<Group>
		{
			new Group ("TJ", "Rachel"),
			//new Group ("James", "Heng"),
			new Group ("Advay", "Heng"),
			new Group ("TJ", "Advay"),
			//new Group ("Steve", "Chris"),
		};

		static int DesiredRounds { get; set; }
		static (int totalRounds, List<Round> bestWorkingSet) BestSolution = (0, new List<Round>());
		//static (int totalRounds, List<Round> bestWorkingSet) BestSolution { get; set; } = (0, new List<Round>());


		static void Main(string[] args)
		{
			// set the DesiredRounds here
			// Default will be Participants.Count - 1
			// but if we have added people later into
			// AlreadyPairedGroups, we should decrease this number
			// since we cannot guarentee the pairings we have already done
			// will give us our desired results

			DesiredRounds = Participants.Count - 1;
			CreateUniquePairing();
		}

		static void CreateUniquePairing()
		{
			var comparer = new PairedComparer();
			var PossiblePairings = new HashSet<Group>(comparer);
			var AlreadyPaired = new HashSet<Group>(comparer);
			var remainingParticipants = new List<string>();

			// Add in all the groups who have already met if any
			foreach (var pairing in AlreadyPairedGroups)
			{
				AlreadyPaired.Add(pairing);
			}

			// Get all possible combinations of participants
			foreach (var participant in Participants)
			{
				foreach (var partner in Participants.Where(p => p != participant))
				{
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

			var isPossible = BackTrackingPairings(groups, 0, new List<Round>());
			while (DesiredRounds > 0 && !BackTrackingPairings(groups, 0, new List<Round>()))
			{
				Console.WriteLine($"Did not find a fully working solution with {DesiredRounds}\nLet's try with {DesiredRounds - 1}");
				DesiredRounds -= 1;
			}
		}

		// We start this method with all the groups, but we can keep calling this
		// method recursively with less groups each time and see if we can produce
		// the correct number of pairings, if not, pop out a recursion and try the next
		// combination!
		static bool BackTrackingPairings(List<Group> groups, int roundNumber, List<Round> workingSet)
		{
			//// base case, we hit enough round numbers
			if (roundNumber == DesiredRounds)
			{
				PrintWorkingSet(workingSet);
				return true;
			}

			var remainingGroups = new List<Group>();
			remainingGroups.AddRange(groups);
			foreach (var group in groups)
			{
				if (IsValidPairing(group, roundNumber, workingSet, out var nextRoundNumber))
				{
					if (!remainingGroups.Remove(group))
						Console.WriteLine("ERROR");

					// add this group to the workingSet's current round
					AddToWorkingSet(workingSet, group, roundNumber);

					if (BackTrackingPairings(remainingGroups, nextRoundNumber, workingSet))
						return true;

					// if we get back here, we should remove the last Group and continue our search
					remainingGroups.Add(group);
					RemoveFromWorkingSet(workingSet, group, roundNumber);
				}
			}

			//// update the BestSolution with current in case we do not find a fully working solution
			//if (roundNumber < DesiredRounds && roundNumber > BestSolution.totalRounds)
			//	BestSolution = (roundNumber, workingSet);

			return false;
		}

		static bool IsValidPairing(Group group, int roundNumber, List<Round> workingSet, out int nextRoundNumber)
		{
			var foundGroups = 0;
			nextRoundNumber = roundNumber;

			// find the corresponding round
			foreach (var round in workingSet)
			{
				if (round.RoundNumber == roundNumber)
				{
					foundGroups = round.Groups.Count;

					// see if the new group can fit inside this round against the existing Groups
					if (!RoundCanGoHere(round.Groups, group))
						return false;
				}
			}

			// if we will be at the max round group size after adding this, increase the groupsize
			if ((foundGroups + 1) == (Participants.Count / 2))
				nextRoundNumber++;

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
			if (x.Partner == y.Partner || x.Partner == y.Person)
				return true;
			if (x.Person == y.Person || x.Person == y.Partner)
				return true;

			return false;
		}

		static bool GroupsAreSame(Group x, Group y)
		{
			if (x is null && y is null)
				return true;
			if (x is null || y is null)
				return false;
			if (x.Partner == y.Partner && x.Person == y.Person)
				return true;
			if (x.Partner == y.Person && x.Person == y.Partner)
				return true;

			return false;
		}

		static void AddToWorkingSet(List<Round> workingset, Group group, int roundNumber)
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

		static void RemoveFromWorkingSet(List<Round> workingset, Group group, int roundNumber)
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

		static void PrintPairings(IEnumerable<Group> groups)
		{
			foreach (var group in groups)
			{
				// change the format so it is easy to paste into the method
				//new Group("Advay", "Heng"),
				Console.WriteLine($"new Group(\"{group.Person}\", \"{group.Partner}\"),");
			}
		}

		static void PrintWorkingSet(List<Round> rounds)
		{
			var roundNumber = 1;
			foreach (var round in rounds)
			{
				Console.WriteLine($"\nRound: {round.RoundNumber + 1}");
				PrintPairings(round.Groups);
				roundNumber++;
			}
		}
	}

	public class Group
	{
		public string Person { get; set; }
		public string Partner { get; set; }

		public Group(string person, string partner)
		{
			Person = person;
			Partner = partner;
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
			if (x.Partner == y.Partner && x.Person == y.Person)
				return true;
			if (x.Partner == y.Person && x.Person == y.Partner)
				return true;

			return false;
		}

		public int GetHashCode([DisallowNull] Group obj)
		{
			var one = string.Compare(obj.Person, obj.Partner);
			if (one == -1)
				return string.Concat(obj.Person, obj.Partner).GetHashCode();

			return string.Concat(obj.Partner, obj.Person).GetHashCode();
		}
	}
}
