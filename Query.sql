SELECT sg.Name, sg.Subject, sg.CreateDate
FROM StudyGroups sg
WHERE EXISTS (
    SELECT 1
    FROM StudyGroupsUsers sgu
    JOIN Users u ON sgu.UserID = u.ID
    WHERE sg.ID = sgu.StudyGroupID
    AND u.Name LIKE 'M%'
)
ORDER BY sg.CreateDate;