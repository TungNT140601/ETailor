SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[GetStaffWithTotalTask]
AS
BEGIN
SELECT s.Id,s.Avatar,s.Fullname,s.Address,s.Phone,s.Role,st.TotalTask
From Staff s INNER JOIN 
(SELECT S.Id, COUNT(P.Id) as TotalTask
FROM Staff S
LEFT JOIN Product P ON (S.Id = P.StaffMakerId AND P.IsActive = 1 AND P.[Status] > 0 AND P.[Status] < 5)
LEFT JOIN [Order] O ON (P.OrderId = O.Id AND O.IsActive = 1 AND O.[Status] > 0 AND O.[Status] < 8)
WHERE S.IsActive = 1
GROUP BY S.Id) as st ON s.Id = st.Id
ORDER BY (st.TotalTask)
END;
GO
