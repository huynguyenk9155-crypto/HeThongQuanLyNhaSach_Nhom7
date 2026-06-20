using System.Text.Json;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Tuan6.Models;
using System.Collections.Generic;
using System.Linq;

namespace Tuan6.Helpers
{
    public static class SessionExtensions
    {
        public static void SetObjectAsJson(this ISession session, string key, object value)
        {
            session.SetString(key, JsonSerializer.Serialize(value));
        }

        public static T? GetObjectFromJson<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default : JsonSerializer.Deserialize<T>(value);
        }

        public static string GetCartKey(ClaimsPrincipal user)
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return string.IsNullOrEmpty(userId) ? "Cart" : $"Cart_{userId}";
        }

        public static string GetPromoCodeKey(ClaimsPrincipal user)
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return string.IsNullOrEmpty(userId) ? "AppliedPromoCode" : $"AppliedPromoCode_{userId}";
        }

        public static string GetDiscountAmountKey(ClaimsPrincipal user)
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return string.IsNullOrEmpty(userId) ? "DiscountAmount" : $"DiscountAmount_{userId}";
        }

        public static void MergeCart(ISession session, string userId)
        {
            var anonCart = session.GetObjectFromJson<List<CartItem>>("Cart");
            if (anonCart != null && anonCart.Any())
            {
                var userCartKey = $"Cart_{userId}";
                var userCart = session.GetObjectFromJson<List<CartItem>>(userCartKey) ?? new List<CartItem>();

                foreach (var anonItem in anonCart)
                {
                    var existingItem = userCart.FirstOrDefault(i => i.BookId == anonItem.BookId);
                    if (existingItem == null)
                    {
                        userCart.Add(anonItem);
                    }
                    else
                    {
                        existingItem.Quantity += anonItem.Quantity;
                    }
                }

                session.SetObjectAsJson(userCartKey, userCart);
                session.Remove("Cart"); // Clear anonymous cart
            }

            // Migrate promo codes if they exist in anonymous session
            var anonPromo = session.GetString("AppliedPromoCode");
            var anonDiscount = session.GetString("DiscountAmount");
            if (!string.IsNullOrEmpty(anonPromo))
            {
                session.SetString($"AppliedPromoCode_{userId}", anonPromo);
                session.Remove("AppliedPromoCode");
            }
            if (!string.IsNullOrEmpty(anonDiscount))
            {
                session.SetString($"DiscountAmount_{userId}", anonDiscount);
                session.Remove("DiscountAmount");
            }
        }
    }
}
