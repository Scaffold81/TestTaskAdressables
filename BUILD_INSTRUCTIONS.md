# Build Instructions for TestTaskAddressables

## Prerequisites

### Unity Setup
- Unity 2023.2+ with WebGL and Android modules
- Addressable Assets package 1.21.19+
- All dependencies installed (Zenject, UniTask, R3, DOTween)

### Platform Requirements
- **WebGL**: Modern browser with WebAssembly support
- **Android**: Android 7.0+ (API Level 24+), ARM64 architecture

## Build Process

### Step 1: Prepare Addressables

1. **Set Active Profile**:
   ```
   Addressables Groups → Profile: Production
   ```

2. **Build Addressables First**:
   ```
   Addressables Groups → Build → New Build → Default Build Script
   ```
   
3. **Verify Build Output**:
   ```
   Check ServerData/WebGL/ folder contains:
   - catalog_*.json
   - catalog_*.hash
   - *.bundle files
   ```

### Step 2: WebGL Build

1. **Platform Settings**:
   ```
   File → Build Settings → WebGL
   Switch Platform if needed
   ```

2. **Player Settings Optimization**:
   ```
   Player Settings → Publishing Settings:
   - Compression Format: Brotli
   - Decompression Fallback: ✓
   - Data Caching: ✓
   - Debug Symbols: ✗
   
   Player Settings → Resolution and Presentation:
   - Run In Background: ✓
   - WebGL Template: Default
   ```

3. **Build WebGL**:
   ```
   Build Settings → Build
   Choose folder: WebGLBuild/
   ```

### Step 3: Android Build

1. **Platform Settings**:
   ```
   File → Build Settings → Android
   Switch Platform if needed
   ```

2. **Player Settings Optimization**:
   ```
   Player Settings → Configuration:
   - Scripting Backend: IL2CPP
   - Target Architectures: ARM64
   - API Compatibility Level: .NET Standard 2.1
   
   Player Settings → Publishing Settings:
   - Build App Bundle (Google Play): ✓
   - Custom Main Gradle Template: ✓
   ```

3. **Build Android**:
   ```
   Build Settings → Build
   Choose: Build App Bundle (.aab) or APK
   ```

## Automated Build Scripts

### Use Editor Menu Items:
```
Addressables → Build → Build WebGL Optimized
Addressables → Build → Build Android Optimized
Addressables → Build → Build Development
```

### Command Line Build:
```bash
# WebGL
Unity.exe -batchmode -quit -projectPath "D:/UnityProjects/TestTaskAdressables" -executeMethod Project.Editor.Build.AddressableBuildScript.BuildWebGLOptimized

# Android
Unity.exe -batchmode -quit -projectPath "D:/UnityProjects/TestTaskAdressables" -executeMethod Project.Editor.Build.AddressableBuildScript.BuildAndroidOptimized
```

## Deployment

### Local Testing
1. **Start Local Server**:
   ```bash
   cd ServerData
   python -m http.server 8080
   ```

2. **Test WebGL**:
   ```
   Open WebGLBuild/index.html in browser
   Verify Addressables load from http://localhost:8080/
   ```

### Production Deployment

1. **Upload Addressables Content**:
   ```
   Upload ServerData/WebGL/ → CDN
   Update Production profile URL to CDN
   ```

2. **Deploy Builds**:
   - **WebGL**: Upload to itch.io, GitHub Pages, or web hosting
   - **Android**: Upload .aab to Google Play Store

## Build Verification Checklist

### WebGL Build
- [ ] Build size < 50MB total
- [ ] Initial load < 30MB
- [ ] Loads in Chrome, Firefox, Safari
- [ ] Addressables download correctly
- [ ] No CORS errors in console
- [ ] Performance acceptable on mid-range devices

### Android Build
- [ ] APK/AAB size < 100MB
- [ ] Initial load < 15MB
- [ ] Works on Android 7.0+
- [ ] Network state changes handled
- [ ] Battery optimization compatible
- [ ] Memory usage within limits

## Performance Targets

### WebGL
- **First load**: < 10 seconds on 3G
- **Scene transition**: < 5 seconds
- **Asset loading**: < 3 seconds per asset
- **Memory usage**: < 512MB

### Android
- **First load**: < 8 seconds on 3G
- **Scene transition**: < 3 seconds
- **Asset loading**: < 2 seconds per asset
- **Memory usage**: < 256MB

## Troubleshooting

### Common WebGL Issues
- **"Cannot load WASM"**: Check server MIME types
- **CORS errors**: Configure server headers
- **Slow loading**: Check compression settings
- **Memory errors**: Reduce texture quality

### Common Android Issues
- **"App not installed"**: Check target API level
- **Network errors**: Test on different networks
- **Performance issues**: Profile on low-end devices
- **Crashes**: Check memory allocation

## Build Size Analysis

After building, analyze bundle sizes:

```
Addressables Groups → Tools → Analyze
- Check Duplicate Bundle Dependencies
- Bundle Layout Preview
- Build Layout Report
```

Target bundle sizes:
- Core_Local: < 5MB
- UI_Remote: < 2MB per bundle
- Characters_Remote: < 10MB
- Effects_Remote: < 3MB per bundle
- Levels_Remote: < 15MB per level

## CI/CD Integration

For automated builds, use Unity Cloud Build or GitHub Actions:

```yaml
# Example GitHub Actions workflow
name: Build Unity Project
on: [push]
jobs:
  build:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v2
    - uses: game-ci/unity-builder@v2
      with:
        targetPlatform: WebGL
        buildMethod: Project.Editor.Build.AddressableBuildScript.BuildWebGLOptimized
```
