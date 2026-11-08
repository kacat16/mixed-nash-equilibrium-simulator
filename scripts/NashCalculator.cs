using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class NashCalculator : Node
{
	public class GameConfig
	{
		public List<PlayerType> PlayerTypes { get; set; } = new List<PlayerType>();
		public List<PayoffMatrix> PayoffMatrices { get; set; } = new List<PayoffMatrix>();

		public GameConfig(List<PlayerType> pTypes, List<PayoffMatrix> pMatrices)
		{
			PlayerTypes = pTypes;
			PayoffMatrices = pMatrices;
		}
	}

	public class PlayerType
	{
		public string Name { get; set; }
		public int NumStrategies { get; set; }
		public List<GamePlayer> Players { get; set; } = new List<GamePlayer>();

		public PlayerType(string pName, int pNumStrategies)
		{
			Name = pName;
			NumStrategies = pNumStrategies;
		}
	}

	public class PayoffMatrix
	{
		public int Player1Type { get; set; }
		public int Player2Type { get; set; }
		public float[,] Values { get; set; }

		public PayoffMatrix(int pType1, int pType2, float[,] pValues)
		{
			Player1Type = pType1;
			Player2Type = pType2;
			Values = pValues;
		}
	}

	public static List<Vector2I> FindPureNashEquilibrium(float[,] payoffMatrix)
	{
		var equilibria = new List<Vector2I>();
		int rows = payoffMatrix.GetLength(0);
		int cols = payoffMatrix.GetLength(1) / 2;

		for (int i = 0; i < rows; i++)
		{
			for (int j = 0; j < cols; j++)
			{
				float p1Payoff = payoffMatrix[i, j * 2];
				float p2Payoff = payoffMatrix[i, j * 2 + 1];

				bool isBestForP1 = true;
				bool isBestForP2 = true;

				for (int k = 0; k < rows; k++)
				{
					if (payoffMatrix[k, j * 2] > p1Payoff)
					{ isBestForP1 = false; break; }
				}

				for (int k = 0; k < cols; k++)
				{
					if (payoffMatrix[i, k * 2 + 1] > p2Payoff)
					{ isBestForP2 = false; break; }
				}

				if (isBestForP1 && isBestForP2)
					equilibria.Add(new Vector2I(i, j));
			}
		}
		return equilibria;
	}

	public static Dictionary<string, object> FindMixedNashEquilibrium2x2(float[,] payoffMatrix)
	{
		var mixedEq = new Dictionary<string, object>
		{
			["player1"] = 0.0f,
			["player2"] = 0.0f
		};
		var expectedPayoffs = new Dictionary<string, object>
		{
			["player1"] = 0.0f,
			["player2"] = 0.0f
		};
		var result = new Dictionary<string, object>
		{
			["mixed_equilibrium"] = mixedEq,
			["expected_payoffs"] = expectedPayoffs
		};

		float a = payoffMatrix[0, 0];
		float b = payoffMatrix[0, 2];
		float c = payoffMatrix[1, 0];
		float d = payoffMatrix[1, 2];

		float p2_a = payoffMatrix[0, 1];
		float p2_b = payoffMatrix[0, 3];
		float p2_c = payoffMatrix[1, 1];
		float p2_d = payoffMatrix[1, 3];

		float denomP1 = (a - b - c + d);
		if (Mathf.Abs(denomP1) > 0.0001f)
		{
			float p1Strategy = (d - b) / denomP1;
			mixedEq["player1"] = Mathf.Clamp(p1Strategy, 0.0f, 1.0f);
		}

		float denomP2 = (p2_a - p2_c - p2_b + p2_d);
		if (Mathf.Abs(denomP2) > 0.0001f)
		{
			float p2Strategy = (p2_d - p2_c) / denomP2;
			mixedEq["player2"] = Mathf.Clamp(p2Strategy, 0.0f, 1.0f);
		}

		return result;
	}

	public static List<Dictionary<string, object>> FindAllNashEquilibria(GameConfig gameConfig)
	{
		var equilibria = new List<Dictionary<string, object>>();

		for (int i = 0; i < gameConfig.PlayerTypes.Count; i++)
		{
			for (int j = i; j < gameConfig.PlayerTypes.Count; j++)
			{
				var equilibrium = new Dictionary<string, object>();
				equilibria.Add(equilibrium);
			}
		}

		return equilibria;
	}
}
