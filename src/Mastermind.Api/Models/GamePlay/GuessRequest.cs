using System.ComponentModel.DataAnnotations;

namespace Mastermind.Api.Models.GamePlay;

public class GuessRequest
{
    [Required] [MinLength(4)] public int[] Guess { get; init; } = [];
}