import cv2
import csv
from gaze_tracking import GazeTracking
import os
current_dir = os.path.dirname(os.path.abspath(__file__))

gaze = GazeTracking()
webcam = cv2.VideoCapture(0)

# Open a CSV file in write mode
with open('eye_gaze_emotion.csv', 'w', newline='') as csvfile:
    fieldnames = ['frame_number', 'left_pupil_x', 'left_pupil_y', 'right_pupil_x', 'right_pupil_y', 'emotion']
    writer = csv.DictWriter(csvfile, fieldnames=fieldnames)

    # Write the header
    writer.writeheader()

    frame_number = 0

    while True:
        ret, frame = webcam.read()
        frame = cv2.resize(frame, (1280, 720))
        if not ret:
            break

        gaze.refresh(frame)
        emotion_detected = None

        # Get the eye pupil coordinates
        left_pupil_coords = gaze.pupil_left_coords()
        right_pupil_coords = gaze.pupil_right_coords()

        # Check if the pupil coordinates are not None
        if left_pupil_coords is not None:
            left_pupil_x, left_pupil_y = left_pupil_coords
        else:
            left_pupil_x, left_pupil_y = None, None

        if right_pupil_coords is not None:
            right_pupil_x, right_pupil_y = right_pupil_coords
        else:
            right_pupil_x, right_pupil_y = None, None

        # Write the coordinates and emotion to the CSV file
        writer.writerow({
            'frame_number': frame_number,
            'left_pupil_x': left_pupil_x,
            'left_pupil_y': left_pupil_y,
            'right_pupil_x': right_pupil_x,
            'right_pupil_y': right_pupil_y,
        })

        # Increment frame number
        frame_number += 1

        new_frame = gaze.annotated_frame()
        text = ""

        if gaze.is_right():
            text = "Looking right"
        elif gaze.is_left():
            text = "Looking left"
        elif gaze.is_center():
            text = "Looking center"

        cv2.putText(new_frame, text, (60, 60), cv2.FONT_HERSHEY_DUPLEX, 2, (255, 0, 0), 2)
        cv2.imshow("Gaze Tracking", new_frame)

        if cv2.waitKey(1) == 27:
            break

# Release the webcam and close the CSV file
webcam.release()
cv2.destroyAllWindows()
