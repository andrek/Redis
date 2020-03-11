using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Redinsgo
{
  class Program
  {
    const ushort QUANTIDADE_PARTICIPANTES = 50;
    const ushort QUANTIDADE_CARTELAS = 99;

    private static string GetKey(string key, int index)
    {
      return $"{key}{index.ToString("00")}";
    }

    static void Main(string[] args)
    {
      IDatabase redis = ConnectionHelper.RedisCache;

      // Cria o set com números de 1 a 99
      for (int i = 1; i <= 99; i++)
        redis.SetAdd("Numeros", i);

      // Cria as cartelas
      for (int i = 1; i <= QUANTIDADE_CARTELAS; i++)
      {
        string cartela = GetKey("cartela", i);

        redis.SetAdd("Cartelas", i);
        redis.KeyDelete(cartela);

        // Sorteia os números da cartela 
        redis.SetAdd(cartela, redis.SetRandomMembers("Numeros", 15));
      }

      Console.WriteLine($"Armazenando informações dos participantes");
      Console.WriteLine();

      for (var i = 1; i <= QUANTIDADE_PARTICIPANTES; i++)
      {
        // Sorteia uma cartela para cada participante
        string cartela = GetKey("cartela", (int)redis.SetPop("Cartelas"));

        string usuario = GetKey("user", i);
        redis.KeyDelete(usuario);

        // Adiciona as informações do participante (nome, cartela, pontuação)
        redis.HashSet(usuario, new HashEntry[] { new HashEntry("name", usuario), new HashEntry("bcartela", cartela), new HashEntry("bscore", 0) });

        Console.WriteLine($"Números do usuário {usuario}: {cartela} - {string.Join(", ", redis.SetMembers(cartela).Select(x => ((int)x).ToString("00")))}");
      }

      // Sorteando os números
      var sorteados = new List<int>();
      var ganhadores = new List<int>();

      while (ganhadores.Count == 0)
      {
        RedisValue numero = redis.SetPop("Numeros");

        sorteados.Add((int)numero);

        for (var i = 1; i <= QUANTIDADE_PARTICIPANTES; i++)
        {
          string usuario = GetKey("user", i);
          string cartela = redis.HashGet(usuario, "bcartela");

          if (redis.SetContains(cartela, numero))
          {
            if (redis.HashIncrement(usuario, "bscore", 1) == 15)
              ganhadores.Add(i);
          }
        }
      }

      Console.WriteLine();
      Console.WriteLine($"Numeros Sorteados: {string.Join(", ", sorteados.OrderBy(x => x).Select(x => x.ToString("00")))}");
      Console.WriteLine();

      int index = ganhadores.First();

      string vencedor = GetKey("user", index);

      Console.WriteLine($"VENCEDOR: {vencedor} - Cartela: {redis.HashGet(vencedor, "bcartela")} - Score: {redis.HashGet(vencedor, "bscore")}");
      Console.WriteLine();

      for (int i = 1; i < QUANTIDADE_PARTICIPANTES; i++)
      {
        string usuario = GetKey("user", i);

        Console.WriteLine($"Usuário: {usuario} - Cartela: {redis.HashGet(usuario, "bcartela")} - Score {redis.HashGet(usuario, "bscore")}");
      }

      Console.ReadKey();
    }
  }
}
