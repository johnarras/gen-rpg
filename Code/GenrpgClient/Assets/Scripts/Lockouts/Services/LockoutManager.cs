using Genrpg.Shared.Interfaces;
using System.Collections.Generic;

namespace Assets.Scripts.Lockouts.Services
{

    public interface ILockoutManager : IInjectable
    {
        void AddLock(long lockoutType, long bits);
        void RemoveLock(long lockoutType, long bits);
        bool HasLock(long lockoutType);
    }


    public class LockoutManager : ILockoutManager
    {

        private Dictionary<long, long> _lockouts = new Dictionary<long, long>();

        public void AddLock(long lockoutType, long bits)
        {
            if (!_lockouts.ContainsKey(lockoutType))
            {
                _lockouts[lockoutType] = 0;
            }
            _lockouts[lockoutType] |= bits;
        }

        public void RemoveLock(long lockoutType, long bits)
        {
            if (!_lockouts.ContainsKey(lockoutType))
            {
                return;
            }

            _lockouts[lockoutType] &= ~bits;
        }

        public bool HasLock(long lockoutType)
        {
            if (!_lockouts.ContainsKey(lockoutType))
            {
                return false;
            }

            return _lockouts[lockoutType] != 0;
        }

    }
}
