SELECT e.entt_id, e.fname, e.lname, e.date_of_birth, e.email_addr, 
		                                    l.postal_code, l.city, l.state, l.postal_code, l.addr_line1,
		                                    p.phone_num
FROM Entity (nolock) as e
INNER JOIN SecurityUser (nolock) sc on sc.oid = e.secu_oid
LEFT JOIN Location (nolock) as l
    on (e.oid = l.entt_oid and e.loca_oid = l.oid)
LEFT JOIN Phone (nolock) as p
    on (e.oid = p.entt_oid)
where sc.user_id  LIKE '{0}%';