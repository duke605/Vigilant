using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vigilant.Utils {
    class Error {
        public static T Try<T>(Func<T> action, T @default, Action<Exception> log = null) {
            try {
                return action.Invoke();
            } catch (Exception ex) {
                log?.Invoke(ex);
                return @default;
            }
        }

        public static async Task<T> TryAsync<T>(Func<Task<T>> action, T @default, Action<Exception> log = null) {
            try {
                return await action.Invoke();
            } catch (Exception ex) {
                log?.Invoke(ex);
                return @default;
            }
        }
    }
}
