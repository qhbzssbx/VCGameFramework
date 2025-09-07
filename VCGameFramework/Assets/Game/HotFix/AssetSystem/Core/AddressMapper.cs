using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.HotFix.AssetSystem.Core
{
    /// <summary>
    /// 默认地址映射器实现
    /// </summary>
    public sealed class AddressMapper : IAddressMapper, IDisposable
    {
        private readonly List<IAddressMappingRule> _rules = new();
        private readonly object _lock = new();
        private AddressMappingContext? _defaultContext;
        private bool _disposed;

        public AddressMapper()
        {
            _defaultContext = AddressMappingContext.CreateDefault();
            Debug.Log($"[AddressMapper] Initialized with default context: Quality={_defaultContext.QualityLevel}, " +
                     $"Device={_defaultContext.DeviceType}, Platform={_defaultContext.Platform}, Region={_defaultContext.RegionCode}");
        }

        public string MapAddress(string package, string originalAddress, AddressMappingContext? context = null)
        {
            ThrowIfDisposed();
            
            var ctx = context ?? _defaultContext ?? AddressMappingContext.CreateDefault();
            var mappedAddress = originalAddress;

            lock (_lock)
            {
                // 按优先级排序并应用规则
                var applicableRules = _rules
                    .Where(rule => rule.IsApplicable(package, originalAddress, ctx))
                    .OrderByDescending(rule => rule.Priority)
                    .ToList();

                foreach (var rule in applicableRules)
                {
                    var previousAddress = mappedAddress;
                    mappedAddress = rule.ApplyMapping(package, mappedAddress, ctx);
                    
                    if (previousAddress != mappedAddress)
                    {
                        Debug.Log($"[AddressMapper] Applied rule {rule.GetType().Name}: {previousAddress} -> {mappedAddress}");
                    }
                }
            }

            if (originalAddress != mappedAddress)
            {
                Debug.Log($"[AddressMapper] Final mapping: {originalAddress} -> {mappedAddress}");
            }

            return mappedAddress;
        }

        public IReadOnlyList<string> MapAddresses(string package, IReadOnlyList<string> addresses, 
            AddressMappingContext? context = null)
        {
            ThrowIfDisposed();
            
            if (addresses == null || addresses.Count == 0)
                return Array.Empty<string>();

            var result = new string[addresses.Count];
            for (int i = 0; i < addresses.Count; i++)
            {
                result[i] = MapAddress(package, addresses[i], context);
            }

            return result;
        }

        public void RegisterMappingRule(IAddressMappingRule rule)
        {
            if (rule == null)
                throw new ArgumentNullException(nameof(rule));

            ThrowIfDisposed();

            lock (_lock)
            {
                if (!_rules.Contains(rule))
                {
                    _rules.Add(rule);
                    Debug.Log($"[AddressMapper] Registered mapping rule: {rule.GetType().Name} (Priority: {rule.Priority})");
                }
            }
        }

        public void UnregisterMappingRule(IAddressMappingRule rule)
        {
            if (rule == null) return;

            ThrowIfDisposed();

            lock (_lock)
            {
                if (_rules.Remove(rule))
                {
                    Debug.Log($"[AddressMapper] Unregistered mapping rule: {rule.GetType().Name}");
                }
            }
        }

        public void ClearMappingRules()
        {
            ThrowIfDisposed();

            lock (_lock)
            {
                var count = _rules.Count;
                _rules.Clear();
                Debug.Log($"[AddressMapper] Cleared {count} mapping rules");
            }
        }

        /// <summary>
        /// 设置默认映射上下文
        /// </summary>
        /// <param name="context">新的默认上下文</param>
        public void SetDefaultContext(AddressMappingContext context)
        {
            ThrowIfDisposed();
            _defaultContext = context ?? AddressMappingContext.CreateDefault();
            Debug.Log($"[AddressMapper] Updated default context: Quality={_defaultContext.QualityLevel}, " +
                     $"Device={_defaultContext.DeviceType}, Platform={_defaultContext.Platform}, Region={_defaultContext.RegionCode}");
        }

        /// <summary>
        /// 获取当前默认上下文
        /// </summary>
        /// <returns>默认上下文的副本</returns>
        public AddressMappingContext GetDefaultContext()
        {
            ThrowIfDisposed();
            return _defaultContext ?? AddressMappingContext.CreateDefault();
        }

        /// <summary>
        /// 获取所有注册的规则信息
        /// </summary>
        /// <returns>规则列表（按优先级降序）</returns>
        public IReadOnlyList<IAddressMappingRule> GetRegisteredRules()
        {
            ThrowIfDisposed();
            
            lock (_lock)
            {
                return _rules.OrderByDescending(r => r.Priority).ToList().AsReadOnly();
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            lock (_lock)
            {
                _rules.Clear();
            }

            Debug.Log("[AddressMapper] Disposed");
        }

        private void ThrowIfDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(AddressMapper));
        }
    }

    /// <summary>
    /// 基于正则表达式的地址映射规则
    /// </summary>
    public sealed class RegexAddressMappingRule : IAddressMappingRule
    {
        public int Priority { get; }
        private readonly string _pattern;
        private readonly string _replacement;
        private readonly System.Text.RegularExpressions.Regex _regex;
        private readonly Func<AddressMappingContext, bool>? _contextPredicate;

        public RegexAddressMappingRule(int priority, string pattern, string replacement, 
            Func<AddressMappingContext, bool>? contextPredicate = null)
        {
            Priority = priority;
            _pattern = pattern;
            _replacement = replacement;
            _contextPredicate = contextPredicate;
            _regex = new System.Text.RegularExpressions.Regex(pattern, System.Text.RegularExpressions.RegexOptions.Compiled);
        }

        public bool IsApplicable(string package, string address, AddressMappingContext context)
        {
            if (!_regex.IsMatch(address)) return false;
            return _contextPredicate?.Invoke(context) ?? true;
        }

        public string ApplyMapping(string package, string address, AddressMappingContext context)
        {
            return _regex.Replace(address, _replacement);
        }

        public override string ToString()
        {
            return $"RegexRule(Priority={Priority}, Pattern='{_pattern}', Replacement='{_replacement}')";
        }
    }

    /// <summary>
    /// 基于前缀的地址映射规则
    /// </summary>
    public sealed class PrefixAddressMappingRule : IAddressMappingRule
    {
        public int Priority { get; }
        private readonly string _prefix;
        private readonly string _newPrefix;
        private readonly Func<AddressMappingContext, bool>? _contextPredicate;

        public PrefixAddressMappingRule(int priority, string prefix, string newPrefix, 
            Func<AddressMappingContext, bool>? contextPredicate = null)
        {
            Priority = priority;
            _prefix = prefix;
            _newPrefix = newPrefix;
            _contextPredicate = contextPredicate;
        }

        public bool IsApplicable(string package, string address, AddressMappingContext context)
        {
            if (!address.StartsWith(_prefix)) return false;
            return _contextPredicate?.Invoke(context) ?? true;
        }

        public string ApplyMapping(string package, string address, AddressMappingContext context)
        {
            return _newPrefix + address.Substring(_prefix.Length);
        }

        public override string ToString()
        {
            return $"PrefixRule(Priority={Priority}, From='{_prefix}', To='{_newPrefix}')";
        }
    }
}