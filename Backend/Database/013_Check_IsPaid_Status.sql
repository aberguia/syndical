-- Vérifier le statut IsPaid des paiements 2022-2023
SELECT 
    Year,
    Month,
    COUNT(*) as 'Nombre de paiements',
    SUM(IF(IsPaid = 1, 1, 0)) as 'Payés',
    SUM(IF(IsPaid = 0, 1, 0)) as 'Non payés'
FROM MonthlyPayments
WHERE Year IN (2022, 2023)
GROUP BY Year, Month
ORDER BY Year DESC, Month DESC
LIMIT 25;

-- Voir quelques exemples détaillés
SELECT 
    mp.Id,
    mp.Year,
    mp.Month,
    mp.Amount,
    mp.IsPaid,
    a.ApartmentNumber,
    b.BuildingNumber
FROM MonthlyPayments mp
JOIN Apartments a ON mp.ApartmentId = a.Id
JOIN Buildings b ON a.BuildingId = b.Id
WHERE mp.Year IN (2022, 2023)
LIMIT 10;
