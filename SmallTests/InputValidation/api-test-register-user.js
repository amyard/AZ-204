import {check} from 'k6';
import http from "k6/http";

export const options = {
    stages: [
        { duration: "10s", target: 20 },
        { duration: "50s", target: 20 }
    ]
}

export default function () {
    const apiUrl = "https://localhost:44393";
    const request = {
        "email": "valid@email.com",
        "dateOfBirth": "2010-12-12",
        "personalInfo": {
            "firstName": "",
            "lastName": ""
        }
    }
    
    const response = http.post(`${apiUrl}/api/register`, JSON.stringify(request), {
        headers: {'Content-Type': 'application/json'}
    });
    
    check(response, {
        'response code was 400': (res) => res.status === 400
    })
}

// choco install k6
// k6 run .\api-test-register-user.js
// https://code-maze.com/aspnetcore-performance-testing-with-k6/
