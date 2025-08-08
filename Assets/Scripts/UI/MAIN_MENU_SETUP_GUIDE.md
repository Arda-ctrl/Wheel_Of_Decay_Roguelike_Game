# Hollow Knight-Style Main Menu Setup Guide

## Overview
This guide explains how to set up the Hollow Knight-style main menu system for your Unity game.

## Required Components

### 1. MainMenuManager Script
- Attach to a GameObject in your main menu scene
- Handles all menu navigation and settings

### 2. UI Panels Structure
Create the following UI panels in your Canvas:

#### Main Menu Panel
- Continue Button
- New Game Button  
- Options Button
- Extras Button
- Quit Button

#### Continue Panel
- Load Game Button
- Clear Save Button
- Back to Main Button
- Save Info Text

#### Options Panel
- Game Options Button
- Audio Options Button
- Video Options Button
- Controller Options Button
- Keyboard Options Button
- Back Button

#### Game Options Panel
- Language Dropdown
- Backer Credits Button
- Show Achievements Button
- Reset Defaults Button
- Back Button

#### Audio Options Panel
- Master Volume Slider
- Sound Volume Slider
- Music Volume Slider
- Reset Defaults Button
- Back Button

#### Video Options Panel
- Resolution Dropdown
- Fullscreen Toggle
- V-Sync Toggle
- Particle Effects Dropdown
- Blur Quality Dropdown
- Brightness Button
- Reset Defaults Button
- Back Button

#### Brightness Panel
- Brightness Slider
- Brightness Value Text
- Back Button

#### Controller Options Panel
- Reset Defaults Button

#### Keyboard Options Panel
- Reset Defaults Button

#### Extras Panel
- Credits Button
- Back Button

#### Credits Panel
- Back Button

#### Quit Confirm Panel
- Confirm Quit Button
- Cancel Button

## Setup Steps

### 1. Create UI Structure
1. Create a Canvas in your main menu scene
2. Create all the panels listed above as child objects
3. Set up the UI elements (buttons, sliders, etc.) in each panel
4. Initially hide all panels except the main menu panel

### 2. Configure MainMenuManager
1. Add the MainMenuManager script to a GameObject
2. Assign all the UI panels to their respective fields in the inspector
3. Assign all buttons, sliders, and other UI elements
4. Set up references to UI_Manager, SaveManager, and AudioManager

### 3. Audio Setup
1. Create AudioSources for music and SFX
2. Set up AudioMixers if you want advanced audio control
3. Assign the audio components to the AudioManager

### 4. Save System Integration
1. Ensure SaveManager is present in the scene
2. The MainMenuManager will automatically find and use it

## Menu Flow

### Main Menu
- **Continue**: Shows save info and load/clear options
- **New Game**: Creates new save and starts game
- **Options**: Opens options menu with categories
- **Extras**: Shows credits and additional content
- **Quit**: Shows quit confirmation

### Options Categories
- **Game**: Language, backer credits, show achievements
- **Audio**: Master, sound, and music volume controls
- **Video**: Resolution, fullscreen, v-sync, effects, brightness panel
- **Controller**: Controller settings and reset
- **Keyboard**: Keyboard settings and reset

### Navigation
- Use Escape key to go back
- Smooth transitions between panels
- Menu history tracking for proper back navigation

## Features

### Save/Load System
- Automatic save detection
- Save info display
- Load game functionality
- Clear save option

### Settings Persistence
- All settings saved to PlayerPrefs
- Automatic loading on startup
- Reset to defaults functionality

### Audio Control
- Master volume control
- Separate music and SFX volume
- Audio mixer integration
- Settings persistence

### Video Settings
- Resolution selection
- Fullscreen toggle
- V-Sync control
- Particle effects quality
- Blur quality settings
- Brightness adjustment

## Customization

### Adding New Options
1. Add UI elements to the appropriate panel
2. Add fields to MainMenuManager
3. Implement the functionality in the settings methods
4. Add to PlayerPrefs for persistence

### Styling
- Use Hollow Knight's dark, atmospheric style
- Implement smooth transitions
- Add hover effects and selection indicators
- Use consistent typography and spacing

### Localization
- Implement language system
- Add text components for all UI elements
- Create localization manager for text switching

## Troubleshooting

### Common Issues
1. **Panels not showing**: Check panel assignments in MainMenuManager
2. **Buttons not working**: Verify button listeners are set up
3. **Settings not saving**: Ensure PlayerPrefs.Save() is called
4. **Audio not working**: Check AudioManager references and AudioSources

### Debug Tips
- Enable debug mode in MainMenuManager
- Check console for error messages
- Verify all required components are assigned
- Test each menu path individually

## Integration with Existing Systems

### UI_Manager Integration
- MainMenuManager works alongside existing UI_Manager
- Use ReturnToMainMenu() to go back to main menu
- Coordinate between game UI and menu UI

### SaveManager Integration
- Automatic save detection and loading
- Save info display
- Clear save functionality

### AudioManager Integration
- Volume control integration
- Settings persistence
- Audio playback during menus

This system provides a complete, Hollow Knight-style main menu with all the requested features and proper integration with your existing game systems. 