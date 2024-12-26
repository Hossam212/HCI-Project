import pandas as pd
import seaborn as sns
import matplotlib.pyplot as plt

# Read the CSV file into a pandas DataFrame
df = pd.read_csv('eye_gaze_emotion.csv')

# Drop rows with missing values in the relevant columns
df.dropna(subset=['left_pupil_x', 'left_pupil_y', 'frame_number'], inplace=True)

# Ensure the pupil coordinates are numeric
df['left_pupil_x'] = pd.to_numeric(df['left_pupil_x'], errors='coerce')
df['left_pupil_y'] = pd.to_numeric(df['left_pupil_y'], errors='coerce')

# Drop rows with invalid coordinates
df.dropna(subset=['left_pupil_x', 'left_pupil_y'], inplace=True)

# Create the pivot table
heatmap_data = df.pivot_table(index='left_pupil_y', columns='left_pupil_x', values='frame_number', aggfunc='count')

# Debugging: Check if the pivot table is empty
if heatmap_data.empty:
    print("Heatmap data is empty. Check your CSV file or filtering logic.")
else:
    # Plot the heatmap
    plt.figure(figsize=(10, 8))
    sns.heatmap(heatmap_data, cmap='YlGnBu', linewidths=0.5, linecolor='grey')
    plt.title('Left Eye Pupil Heatmap')
    plt.xlabel('X Coordinate')
    plt.ylabel('Y Coordinate')
    plt.show()
