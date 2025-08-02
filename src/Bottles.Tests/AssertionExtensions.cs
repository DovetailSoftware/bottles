using System.Collections.Generic;
using System.Linq;

namespace Shouldly
{
    public static class AssertionExtensions
    {
        public static void ShouldEqual<T>(this T actual, T expected)
        {
            actual.ShouldBe(expected);
        }

        public static void ShouldBeTheSameAs(this object actual, object expected)
        {
            actual.ShouldBe(expected);
        }

        public static void ShouldHaveTheSameElementsAs<T>(this IEnumerable<T> actual, params T[] expected)
        {
            var listActual = actual.ToList();
            var listExpected = expected.ToList();

            var firstNotSecond = listActual.Except(listExpected).ToList();
            var secondNotFirst = listExpected.Except(listActual).ToList();

            if (firstNotSecond.Any() || secondNotFirst.Any())
            {
                var message = "The collections do not have the same elements.\n" +
                              $"Elements in the first collection but not in the second: {string.Join(", ", firstNotSecond)}\n" +
                              $"Elements in the second collection but not in the first: {string.Join(", ", secondNotFirst)}";
                throw new ShouldAssertException(message);
            }
        }
    }
}