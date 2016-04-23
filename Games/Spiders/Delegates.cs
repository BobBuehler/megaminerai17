using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Joueur.cs.Games.Spiders
{
    /// <summary>
    /// A function that outputs a parameter,
    /// and returns true or false for success or failure.
    /// </summary>
    /// <typeparam name="T">The type of output.</typeparam>
    /// <param name="output">The output.</param>
    /// <returns>True or false for success or failure.</returns>
    public delegate bool TryOut<T>(out T output);

    /// <summary>
    /// A function that takes a parameter, outputs a parameter,
    /// and returns true or false for success or failure.
    /// </summary>
    /// <typeparam name="TIn">The type of input.</typeparam>
    /// <typeparam name="TOut">The type of output.</typeparam>
    /// <param name="input">The input.</param>
    /// <param name="output">The output.</param>
    /// <returns>True or false for success or failure.</returns>
    public delegate bool TryOut<TIn, TOut>(TIn input, out TOut output);

}
