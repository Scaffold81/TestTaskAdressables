# Addressable Test Scenarios

## Overview
This document describes test scenarios for the Addressable system implementation.

## Test Environment Setup

### 1. Local Development Server
```bash
cd ServerData
python -m http.server 8080
```
Access: http://localhost:8080/WebGL/

### 2. Unity Profiles
- **Development**: `http://localhost:8080/[BuildTarget]`
- **Staging**: `https://staging.yourcdn.com/[BuildTarget]`
- **Production**: `https://cdn.yourprod.com/[BuildTarget]`

## Test Scenarios

### Scenario 1: First Launch (No Cache)
**Objective**: Test initial download and caching behavior

**Steps**:
1. Clear all cache in Unity and browser
2. Start application with Development profile
3. Monitor network traffic and download times
4. Verify Core_Local assets load immediately
5. Test remote asset loading

**Expected Results**:
- Core assets available immediately
- Remote assets download on demand
- Progress indicators work correctly
- Total download < 30MB for WebGL, < 15MB for Android

### Scenario 2: Content Update Without App Release
**Objective**: Verify content updates work without app store release

**Steps**:
1. Build initial version with test sprite
2. Replace test sprite in AddressableAssets/Effects/
3. Build Addressables only (not full app)
4. Upload to server
5. Restart app - should download new sprite

**Expected Results**:
- App detects content update
- Downloads only changed assets
- New sprite appears without app update

### Scenario 3: Network Error Recovery
**Objective**: Test resilience to network failures

**Steps**:
1. Start loading large asset (character prefab)
2. Disconnect internet during download
3. Reconnect after 30 seconds
4. Verify automatic retry behavior

**Expected Results**:
- Loading pauses gracefully
- Automatic retry with exponential backoff
- User sees appropriate error messages
- Download resumes from where it left off

### Scenario 4: Profile Switching
**Objective**: Test profile switching without restart

**Steps**:
1. Load assets with Development profile
2. Use DevOverlay to switch to Staging profile
3. Clear cache and reload assets
4. Verify assets load from new CDN

**Expected Results**:
- Profile switch works in runtime
- Assets load from correct server
- No corruption or conflicts

### Scenario 5: Memory Management
**Objective**: Verify proper memory cleanup

**Steps**:
1. Load multiple large assets
2. Navigate between scenes
3. Force garbage collection
4. Monitor memory usage

**Expected Results**:
- Unused assets released automatically
- Memory usage stays within limits
- No memory leaks detected

## Performance Benchmarks

### WebGL Targets
- **First load time**: < 10 seconds on 3G
- **Initial download**: < 30MB
- **Individual asset load**: < 3 seconds
- **Scene transition**: < 5 seconds

### Android Targets  
- **First load time**: < 8 seconds on 3G
- **Initial download**: < 15MB
- **Individual asset load**: < 2 seconds
- **Scene transition**: < 3 seconds

## Automated Testing

### Unity Test Runner
Use `AddressableTestController.RunAutomatedTestSequence()` for CI/CD integration.

### Test Coverage
- [x] Asset loading (sprites, prefabs, scenes)
- [x] Download size calculation
- [x] Content updates
- [x] Cache management
- [x] Memory cleanup
- [x] Error handling
- [x] Profile switching

## Known Issues & Limitations

### WebGL Limitations
- No persistent cache between sessions
- CORS headers required for remote assets
- Limited concurrent connections (6 max)

### Android Limitations
- Network state changes can interrupt downloads
- Storage permissions required for cache
- Battery optimization may affect background downloads

## Troubleshooting

### Common Issues
1. **"Missing Reference" errors**: Check asset keys match exactly
2. **Slow loading**: Verify compression settings and bundle size
3. **CORS errors**: Configure server headers correctly
4. **Cache not working**: Check platform cache permissions

### Debug Tools
- DevOverlay panel for runtime debugging
- Unity Memory Profiler for leak detection
- Browser DevTools for WebGL network analysis
- Android Profiler for mobile performance

## Manual Test Checklist

### Basic Functionality
- [ ] Assets load correctly by key
- [ ] Progress bars work during loading
- [ ] Cache persists between sessions
- [ ] Memory releases after scene changes

### Content Updates
- [ ] Update detection works
- [ ] Only changed assets download
- [ ] App continues working during updates
- [ ] Rollback works if update fails

### Error Handling
- [ ] Graceful degradation with no internet
- [ ] Retry logic works correctly
- [ ] User sees meaningful error messages
- [ ] App doesn't crash on network errors

### Platform Specific
- [ ] WebGL builds work in all major browsers
- [ ] Android builds work on different screen sizes
- [ ] Performance meets target benchmarks
- [ ] Memory usage stays within limits

## Reporting

After testing, document:
1. Actual performance metrics vs targets
2. Bundle sizes and optimization results
3. Network usage patterns
4. Memory allocation graphs
5. User experience feedback
6. Technical debt and improvement opportunities
